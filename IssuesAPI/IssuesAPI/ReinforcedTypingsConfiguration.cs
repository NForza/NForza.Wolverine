using NForza.Wolverine;
using NForza.Wolverine.ValueTypes;
using Reinforced.Typings.Ast.TypeNames;
using Reinforced.Typings.Fluent;

[assembly: Reinforced.Typings.Attributes.TsGlobal(
    CamelCaseForProperties = true,
    UseModules = true,
    DiscardNamespacesWhenUsingModules = true,
    AutoOptionalProperties = true,
    WriteWarningComment = false
)]

namespace Wolverine.Issues;

public static class ReinforcedTypingsConfiguration
{
    public static void Configure(Reinforced.Typings.Fluent.ConfigurationBuilder builder)
    {
        var contractsAssembly = typeof(Contracts.Issues.IssueCreated).Assembly;

        // Auto-substitute all value types to string
        var valueTypes = contractsAssembly.GetTypes()
            .Where(t => t.IsValueType && typeof(IValueType).IsAssignableFrom(t));

        foreach (var vt in valueTypes)
        {
            builder.Substitute(vt, new RtSimpleTypeName("string"));
        }

        // DateTimeOffset serializes as ISO 8601 string
        builder.Substitute(typeof(DateTimeOffset), new RtSimpleTypeName("string"));

        // Auto-export all record classes from the Contracts assembly
        var recordTypes = contractsAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.GetMethod("<Clone>$") != null)
            .ToArray();

        builder.ExportAsInterfaces(recordTypes, config => config
            .WithPublicProperties()
            .AutoI(false)
        );

        // Exclude source-generated infrastructure types from RT export
        var wolverineInfraTypes = typeof(ReinforcedTypingsConfiguration).Assembly.GetTypes()
            .Where(t => t.Namespace == "NForza.Wolverine");
        foreach (var t in wolverineInfraTypes)
        {
            builder.Substitute(t, new RtSimpleTypeName("any"));
        }

        // Discover WolverineHub subclasses in the API assembly and generate SignalR services
        var apiAssembly = typeof(ReinforcedTypingsConfiguration).Assembly;
        var hubTypes = apiAssembly.GetTypes()
            .Where(t => !t.IsAbstract && IsSubclassOfWolverineHub(t))
            .ToArray();

        foreach (var hubType in hubTypes)
        {
            var hub = (WolverineHub)Activator.CreateInstance(hubType)!;
            HubMetadataStore.Register(hubType, hub);
        }

        if (hubTypes.Length > 0)
        {
            builder.ExportAsClasses(hubTypes, config =>
            {
                config.WithCodeGenerator<SignalRHubServiceGenerator>();
                config.DontIncludeToNamespace();
            });
        }
    }

    private static bool IsSubclassOfWolverineHub(Type t)
    {
        var baseType = t.BaseType;
        while (baseType != null)
        {
            if (baseType.Name == "WolverineHub")
                return true;
            baseType = baseType.BaseType;
        }
        return false;
    }
}
