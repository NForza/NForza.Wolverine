using System.Collections.Immutable;
using System.Text;

namespace NForza.Wolverine.Generators.CodeGeneration;

internal static class OpenApiTransformerTemplates
{
    public static string Generate(ImmutableArray<ValueTypeInfo> valueTypes)
    {
        var sb = new StringBuilder();
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using System.Threading;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine("using Microsoft.AspNetCore.OpenApi;");
        sb.AppendLine("using Microsoft.OpenApi.Models;");
        sb.AppendLine();
        sb.AppendLine("namespace NForza.Wolverine.ValueTypes;");
        sb.AppendLine();
        sb.AppendLine("public class WolverineValueTypeOpenApiTransformer : IOpenApiSchemaTransformer");
        sb.AppendLine("{");
        sb.AppendLine("    public Task TransformAsync(OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)");
        sb.AppendLine("    {");
        sb.AppendLine("        var type = context.JsonTypeInfo.Type;");

        bool first = true;
        foreach (var vt in valueTypes)
        {
            var fqn = vt.FullyQualifiedName;
            var (schemaType, format) = GetOpenApiTypeAndFormat(vt);
            var keyword = first ? "if" : "else if";
            first = false;

            sb.AppendLine($"        {keyword} (type == typeof({fqn}))");
            sb.AppendLine("        {");
            sb.AppendLine($"            context.Schema.Type = \"{schemaType}\";");
            if (format is not null)
                sb.AppendLine($"            context.Schema.Format = \"{format}\";");
            sb.AppendLine("        }");
        }

        sb.AppendLine("        return Task.CompletedTask;");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static (string type, string? format) GetOpenApiTypeAndFormat(ValueTypeInfo vt)
    {
        return vt.Kind switch
        {
            ValueTypeKind.Guid => ("string", "uuid"),
            ValueTypeKind.String => ("string", null),
            ValueTypeKind.Int => ("integer", "int32"),
            ValueTypeKind.Long => ("integer", "int64"),
            ValueTypeKind.Double => ("number", "double"),
            ValueTypeKind.Decimal => ("number", "double"),
            ValueTypeKind.DateOnly => ("string", "date"),
            ValueTypeKind.DateTime => ("string", "date-time"),
            ValueTypeKind.DateTimeOffset => ("string", "date-time"),
            _ => ("string", null)
        };
    }
}
