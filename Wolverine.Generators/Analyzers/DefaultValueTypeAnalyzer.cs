using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NForza.Wolverine.Generators.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DefaultValueTypeAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "WVTN001";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        "Avoid default value type",
        "Avoid using default({0}) — it creates an invalid value type. Use a constructor or factory method instead.",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly string[] ValueTypeAttributeNames =
    [
        "GuidValueAttribute",
        "StringValueAttribute",
        "IntValueAttribute",
        "DoubleValueAttribute",
        "LongValueAttribute",
        "DecimalValueAttribute",
        "DateOnlyValueAttribute",
        "DateTimeValueAttribute",
        "DateTimeOffsetValueAttribute"
    ];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // default(MyType) expressions
        context.RegisterSyntaxNodeAction(AnalyzeDefaultExpression, SyntaxKind.DefaultExpression);

        // MyType x = default; (default literal)
        context.RegisterSyntaxNodeAction(AnalyzeDefaultLiteral, SyntaxKind.DefaultLiteralExpression);
    }

    private void AnalyzeDefaultExpression(SyntaxNodeAnalysisContext context)
    {
        var defaultExpr = (DefaultExpressionSyntax)context.Node;
        var typeInfo = context.SemanticModel.GetTypeInfo(defaultExpr, context.CancellationToken);
        CheckType(context, typeInfo.Type, defaultExpr.GetLocation());
    }

    private void AnalyzeDefaultLiteral(SyntaxNodeAnalysisContext context)
    {
        var literal = (LiteralExpressionSyntax)context.Node;
        var typeInfo = context.SemanticModel.GetTypeInfo(literal, context.CancellationToken);
        CheckType(context, typeInfo.ConvertedType, literal.GetLocation());
    }

    private static void CheckType(SyntaxNodeAnalysisContext context, ITypeSymbol? type, Location location)
    {
        if (type is not INamedTypeSymbol namedType) return;
        if (!namedType.IsValueType) return;

        var hasValueTypeAttribute = namedType.GetAttributes()
            .Any(a => a.AttributeClass is not null &&
                      ValueTypeAttributeNames.Contains(a.AttributeClass.Name));

        if (hasValueTypeAttribute)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, location, namedType.Name));
        }
    }
}
