using System.Collections.Generic;
using System.Text;

namespace NForza.Wolverine.ValueTypes.Generators.CodeGeneration;

internal static class StringValueTemplates
{
    private const string RecordStructTemplate = @"#nullable enable
using System;
using System.Diagnostics;
using System.Text.Json.Serialization;
using NForza.Wolverine.ValueTypes;

{{NamespaceDeclaration}}
[JsonConverter(typeof({{Name}}JsonConverter))]
[DebuggerDisplay(""{Value}"")]
public partial record struct {{Name}}(string Value) : IStringValueType
{
    public static readonly {{Name}} Empty = new {{Name}}(string.Empty);
    public bool IsEmpty() => string.IsNullOrEmpty(Value);
    public static implicit operator string({{Name}} typedId) => typedId.Value;
    public static explicit operator {{Name}}(string value) => new(value);
    public string AsString() => Value;
    public bool IsValid() => !string.IsNullOrEmpty(Value){{ValidationSuffix}};
    public override string ToString() => Value ?? string.Empty;

    public static bool TryParse(string? s, out {{Name}} result)
    {
        if (s is not null)
        {
            result = new {{Name}}(s);
            return true;
        }
        result = default;
        return false;
    }

    public static bool TryParse(string? s, IFormatProvider? provider, out {{Name}} result)
        => TryParse(s, out result);
}";

    public static string GenerateRecordStruct(ValueTypeInfo info)
    {
        var namespaceDecl = string.IsNullOrEmpty(info.Namespace) ? "" : $"namespace {info.Namespace};\n";

        var validationParts = new StringBuilder();
        if (info.MinLength.HasValue)
            validationParts.Append($" && Value.Length >= {info.MinLength.Value}");
        if (info.MaxLength.HasValue)
            validationParts.Append($" && Value.Length <= {info.MaxLength.Value}");
        if (!string.IsNullOrEmpty(info.ValidationRegex))
            validationParts.Append($" && System.Text.RegularExpressions.Regex.IsMatch(Value, \"{info.ValidationRegex}\")");

        return TemplateEngine.Render(RecordStructTemplate, new Dictionary<string, string>
        {
            ["Name"] = info.Name,
            ["NamespaceDeclaration"] = namespaceDecl,
            ["ValidationSuffix"] = validationParts.ToString()
        });
    }
}
