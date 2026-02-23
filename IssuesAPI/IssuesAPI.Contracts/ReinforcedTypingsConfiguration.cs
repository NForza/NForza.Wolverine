using NForza.Wolverine.ValueTypes;
using Reinforced.Typings.Ast.TypeNames;
using Reinforced.Typings.Fluent;

[assembly: Reinforced.Typings.Attributes.TsGlobal(
    CamelCaseForProperties = true,
    UseModules = true,
    AutoOptionalProperties = true,
    WriteWarningComment = false
)]

namespace Wolverine.Issues.Contracts;

public static class ReinforcedTypingsConfiguration
{
    public static void Configure(ConfigurationBuilder builder)
    {
        var assembly = typeof(ReinforcedTypingsConfiguration).Assembly;

        // Auto-substitute all value types to string
        var valueTypes = assembly.GetTypes()
            .Where(t => t.IsValueType && typeof(IValueType).IsAssignableFrom(t));

        foreach (var vt in valueTypes)
        {
            builder.Substitute(vt, new RtSimpleTypeName("string"));
        }

        // DateTimeOffset serializes as ISO 8601 string
        builder.Substitute(typeof(DateTimeOffset), new RtSimpleTypeName("string"));

        // Auto-export all record classes from this assembly
        var recordTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.GetMethod("<Clone>$") != null)
            .ToArray();

        builder.ExportAsInterfaces(recordTypes, config => config
            .WithPublicProperties()
            .AutoI(false)
        );
    }
}
