using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NForza.Wolverine.Generators.CodeGeneration;
using NForza.Wolverine.Generators.Roslyn;

namespace NForza.Wolverine.Generators;

[Generator]
public class WolverineValueTypeGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var guidValues = CreateProvider(context, Roslyn.SyntaxNodeExtensions.IsRecordWithGuidValueAttribute, ValueTypeKind.Guid);
        var stringValues = CreateProvider(context, Roslyn.SyntaxNodeExtensions.IsRecordWithStringValueAttribute, ValueTypeKind.String);
        var intValues = CreateProvider(context, Roslyn.SyntaxNodeExtensions.IsRecordWithIntValueAttribute, ValueTypeKind.Int);
        var doubleValues = CreateProvider(context, Roslyn.SyntaxNodeExtensions.IsRecordWithDoubleValueAttribute, ValueTypeKind.Double);

        var allValues = guidValues
            .Combine(stringValues)
            .Combine(intValues)
            .Combine(doubleValues)
            .Select((combined, _) =>
            {
                var (((guids, strings), ints), doubles) = combined;
                return guids.AddRange(strings).AddRange(ints).AddRange(doubles);
            });

        context.RegisterSourceOutput(allValues, GenerateAllSources);

        // Generate Marten extension methods independently, whenever
        // both Marten and the ValueTypes abstractions are referenced.
        var shouldGenerateMartenExtensions = context.CompilationProvider.Select((compilation, _) =>
            compilation.GetTypeByMetadataName("Marten.Events.IQueryEventStore") is not null
            && compilation.GetTypeByMetadataName("NForza.Wolverine.ValueTypes.IGuidValueType") is not null);

        context.RegisterSourceOutput(shouldGenerateMartenExtensions, GenerateMartenExtensions);

        // Emit WolverineHub base class so user code can inherit from it.
        context.RegisterPostInitializationOutput(EmitWolverineHub);

        // SignalR bridge generation: find WolverineHub subclasses, parse constructor DSL.
        var signalRHubs = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (node, _) => node.IsClassInheritingFromWolverineHub(),
                transform: (ctx, _) => ExtractSignalRHubInfo(ctx))
            .Where(x => x is not null)
            .Select((x, _) => x!)
            .Collect();

        var signalRHubsWithCheck = signalRHubs.Combine(
            context.CompilationProvider.Select((compilation, _) =>
                compilation.GetTypeByMetadataName("Microsoft.AspNetCore.SignalR.Hub") is not null));

        context.RegisterSourceOutput(signalRHubsWithCheck, GenerateSignalRBridges);
    }

    private IncrementalValueProvider<ImmutableArray<ValueTypeInfo>> CreateProvider(
        IncrementalGeneratorInitializationContext context,
        Func<SyntaxNode, bool> predicate,
        ValueTypeKind kind)
    {
        return context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (node, _) => predicate(node),
                transform: (ctx, _) => ExtractValueTypeInfo(ctx, kind))
            .Where(x => x is not null)
            .Select((x, _) => x!)
            .Collect();
    }

    private ValueTypeInfo? ExtractValueTypeInfo(GeneratorSyntaxContext context, ValueTypeKind kind)
    {
        var symbol = context.GetRecordSymbolFromContext();
        if (symbol is null) return null;

        var info = new ValueTypeInfo
        {
            Name = symbol.Name,
            Namespace = symbol.ContainingNamespace.GetNameOrEmpty(),
            Kind = kind
        };

        switch (kind)
        {
            case ValueTypeKind.String:
                ExtractStringValidation(symbol, info);
                break;
            case ValueTypeKind.Int:
                ExtractIntValidation(symbol, info);
                break;
            case ValueTypeKind.Double:
                ExtractDoubleValidation(symbol, info);
                break;
        }

        return info;
    }

    private void ExtractStringValidation(INamedTypeSymbol symbol, ValueTypeInfo info)
    {
        var attribute = symbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "StringValueAttribute");
        if (attribute is null) return;

        var args = attribute.ConstructorArguments.Select(a => a.Value?.ToString()).ToList();
        if (args.Count > 0 && int.TryParse(args[0], out var min) && min >= 0) info.MinLength = min;
        if (args.Count > 1 && int.TryParse(args[1], out var max) && max >= 0) info.MaxLength = max;
        if (args.Count > 2) info.ValidationRegex = args[2];
    }

    private void ExtractIntValidation(INamedTypeSymbol symbol, ValueTypeInfo info)
    {
        var attribute = symbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "IntValueAttribute");
        if (attribute is null) return;

        var args = attribute.ConstructorArguments.Select(a => a.Value?.ToString()).ToList();
        if (args.Count > 0 && int.TryParse(args[0], out var min) && min != int.MinValue) info.IntMinimum = min;
        if (args.Count > 1 && int.TryParse(args[1], out var max) && max != int.MaxValue) info.IntMaximum = max;
    }

    private void ExtractDoubleValidation(INamedTypeSymbol symbol, ValueTypeInfo info)
    {
        var attribute = symbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "DoubleValueAttribute");
        if (attribute is null) return;

        var args = attribute.ConstructorArguments.Select(a => a.Value?.ToString()).ToList();
        if (args.Count > 0 && double.TryParse(args[0], out var min) && !double.IsNaN(min)) info.DoubleMinimum = min;
        if (args.Count > 1 && double.TryParse(args[1], out var max) && !double.IsNaN(max)) info.DoubleMaximum = max;
    }

    private void GenerateAllSources(SourceProductionContext spc, ImmutableArray<ValueTypeInfo> valueTypes)
    {
        foreach (var vt in valueTypes)
        {
            var recordSource = GenerateRecordStruct(vt);
            spc.AddSource($"{vt.Name}.g.cs", recordSource);

            var jsonConverterSource = GenerateJsonConverter(vt);
            spc.AddSource($"{vt.Name}JsonConverter.g.cs", jsonConverterSource);
        }

        if (valueTypes.Length > 0)
        {
            var extensionSource = WolverineExtensionTemplates.Generate(valueTypes);
            spc.AddSource("WolverineValueTypeExtension.g.cs", extensionSource);
        }
    }

    private void GenerateMartenExtensions(SourceProductionContext spc, bool shouldGenerate)
    {
        if (shouldGenerate)
        {
            spc.AddSource("MartenGuidValueTypeExtensions.g.cs", MartenExtensionTemplates.Generate());
        }
    }

    private string GenerateRecordStruct(ValueTypeInfo vt)
    {
        return vt.Kind switch
        {
            ValueTypeKind.Guid => GuidValueTemplates.GenerateRecordStruct(vt),
            ValueTypeKind.String => StringValueTemplates.GenerateRecordStruct(vt),
            ValueTypeKind.Int => IntValueTemplates.GenerateRecordStruct(vt),
            ValueTypeKind.Double => DoubleValueTemplates.GenerateRecordStruct(vt),
            _ => throw new InvalidOperationException($"Unknown value type kind: {vt.Kind}")
        };
    }

    private string GenerateJsonConverter(ValueTypeInfo vt)
    {
        return vt.Kind switch
        {
            ValueTypeKind.Guid => JsonConverterTemplates.GenerateGuid(vt),
            ValueTypeKind.String => JsonConverterTemplates.GenerateString(vt),
            ValueTypeKind.Int => JsonConverterTemplates.GenerateInt(vt),
            ValueTypeKind.Double => JsonConverterTemplates.GenerateDouble(vt),
            _ => throw new InvalidOperationException($"Unknown value type kind: {vt.Kind}")
        };
    }

    // --- SignalR bridge generation ---

    private const string WolverineHubSource = @"#nullable enable
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace NForza.Wolverine;

internal record WolverineHubEventConfig(string EventTypeName, string EventTypeNamespace, bool IsBroadcast, string? GroupKeyProperty);

internal abstract class WolverineHub
{
    public string Path { get; private set; } = """";
    public List<WolverineHubEventConfig> Events { get; } = new();

    protected void UsePath(string path) => Path = path;

    protected void Broadcast<T>()
        => Events.Add(new WolverineHubEventConfig(typeof(T).Name, typeof(T).Namespace ?? """", true, null));

    protected void SendToGroup<T>(Expression<Func<T, object>> groupKeySelector)
    {
        string? groupKey = null;
        if (groupKeySelector.Body is MemberExpression member)
            groupKey = member.Member.Name;
        else if (groupKeySelector.Body is UnaryExpression unary && unary.Operand is MemberExpression innerMember)
            groupKey = innerMember.Member.Name;
        Events.Add(new WolverineHubEventConfig(typeof(T).Name, typeof(T).Namespace ?? """", false, groupKey));
    }
}
";

    private void EmitWolverineHub(IncrementalGeneratorPostInitializationContext context)
    {
        context.AddSource("WolverineHub.g.cs", WolverineHubSource);
    }

    private SignalRHubInfo? ExtractSignalRHubInfo(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var symbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;
        if (symbol is null) return null;

        var constructorBody = classDeclaration.Members
            .OfType<ConstructorDeclarationSyntax>()
            .FirstOrDefault()?.Body;

        if (constructorBody is null) return null;

        string? path = null;
        var events = new List<SignalREventInfo>();

        foreach (var statement in constructorBody.Statements)
        {
            if (statement is not ExpressionStatementSyntax expressionStatement) continue;
            if (expressionStatement.Expression is not InvocationExpressionSyntax invocation) continue;

            var methodName = invocation.Expression switch
            {
                IdentifierNameSyntax id => id.Identifier.Text,
                GenericNameSyntax generic => generic.Identifier.Text,
                _ => null
            };

            if (methodName == "UsePath" && invocation.ArgumentList.Arguments.Count == 1)
            {
                var arg = invocation.ArgumentList.Arguments[0].Expression;
                if (arg is LiteralExpressionSyntax literal)
                {
                    path = literal.Token.ValueText;
                }
            }
            else if (methodName == "Broadcast" && invocation.Expression is GenericNameSyntax broadcastGeneric)
            {
                var typeArg = broadcastGeneric.TypeArgumentList.Arguments.FirstOrDefault();
                if (typeArg is not null)
                {
                    var eventSymbol = context.SemanticModel.GetSymbolInfo(typeArg).Symbol as INamedTypeSymbol;
                    if (eventSymbol is not null)
                    {
                        events.Add(new SignalREventInfo
                        {
                            EventTypeName = eventSymbol.Name,
                            EventTypeNamespace = eventSymbol.ContainingNamespace.GetNameOrEmpty(),
                            IsBroadcast = true
                        });
                    }
                }
            }
            else if (methodName == "SendToGroup" && invocation.Expression is GenericNameSyntax emitGeneric)
            {
                var typeArg = emitGeneric.TypeArgumentList.Arguments.FirstOrDefault();
                if (typeArg is not null)
                {
                    var eventSymbol = context.SemanticModel.GetSymbolInfo(typeArg).Symbol as INamedTypeSymbol;
                    if (eventSymbol is not null)
                    {
                        string? groupKeyProperty = null;

                        // Parse lambda: e => e.PropertyName
                        if (invocation.ArgumentList.Arguments.Count == 1)
                        {
                            var lambdaArg = invocation.ArgumentList.Arguments[0].Expression;
                            if (lambdaArg is SimpleLambdaExpressionSyntax lambda &&
                                lambda.Body is MemberAccessExpressionSyntax memberAccess)
                            {
                                groupKeyProperty = memberAccess.Name.Identifier.Text;
                            }
                        }

                        events.Add(new SignalREventInfo
                        {
                            EventTypeName = eventSymbol.Name,
                            EventTypeNamespace = eventSymbol.ContainingNamespace.GetNameOrEmpty(),
                            IsBroadcast = false,
                            GroupKeyProperty = groupKeyProperty
                        });
                    }
                }
            }
        }

        if (string.IsNullOrEmpty(path) || events.Count == 0) return null;

        return new SignalRHubInfo
        {
            HubClassName = symbol.Name,
            Namespace = symbol.ContainingNamespace.GetNameOrEmpty(),
            Path = path!,
            Events = events.ToImmutableArray()
        };
    }

    private void GenerateSignalRBridges(
        SourceProductionContext spc,
        (ImmutableArray<SignalRHubInfo> Hubs, bool SignalRAvailable) input)
    {
        if (!input.SignalRAvailable || input.Hubs.Length == 0) return;

        foreach (var hub in input.Hubs)
        {
            spc.AddSource($"{hub.GeneratedHubName}.g.cs", SignalRBridgeTemplates.GenerateHub(hub));
            spc.AddSource($"{hub.HubClassName}SignalRBridge.g.cs", SignalRBridgeTemplates.GenerateBridge(hub));
        }

        spc.AddSource("WolverineHubExtensions.g.cs", SignalRBridgeTemplates.GenerateRegistration(input.Hubs));
    }

}
