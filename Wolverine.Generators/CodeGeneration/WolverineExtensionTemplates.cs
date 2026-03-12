using System.Collections.Immutable;
using System.Text;

namespace NForza.Wolverine.Generators.CodeGeneration;

internal static class WolverineExtensionTemplates
{
    public static string Generate(ImmutableArray<ValueTypeInfo> valueTypes)
    {
        var sb = new StringBuilder();
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using System.Text.Json;");
        sb.AppendLine("using Wolverine;");
        sb.AppendLine();
        sb.AppendLine("namespace NForza.Wolverine.ValueTypes;");
        sb.AppendLine();
        sb.AppendLine("public class WolverineValueTypeExtension : IWolverineExtension");
        sb.AppendLine("{");
        sb.AppendLine("    public void Configure(WolverineOptions options)");
        sb.AppendLine("    {");
        sb.AppendLine("        options.UseSystemTextJsonForSerialization(jsonOptions =>");
        sb.AppendLine("        {");

        foreach (var vt in valueTypes)
        {
            var fqn = string.IsNullOrEmpty(vt.Namespace) ? vt.Name : vt.Namespace + "." + vt.Name;
            sb.AppendLine($"            jsonOptions.Converters.Add(new {fqn}JsonConverter());");
        }

        sb.AppendLine("        });");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }
}
