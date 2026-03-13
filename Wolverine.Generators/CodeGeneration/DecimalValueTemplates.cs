using System.Collections.Generic;
using System.Globalization;

namespace NForza.Wolverine.Generators.CodeGeneration;

internal static class DecimalValueTemplates
{
    private const string RecordStructTemplate = @"#nullable enable
using System;
using System.Diagnostics;
using System.Text.Json.Serialization;
using NForza.Wolverine.ValueTypes;

{{NamespaceDeclaration}}
[JsonConverter(typeof({{Name}}JsonConverter))]
[DebuggerDisplay(""{Value}"")]
public partial record struct {{Name}}(decimal Value) : IDecimalValueType, IComparable, IComparable<{{Name}}>, IEquatable<{{Name}}>
#if NET7_0_OR_GREATER
    , IParsable<{{Name}}>
#endif
{
    public int CompareTo(object? other) => other is {{Name}} ? Value.CompareTo((({{Name}})other).Value) : -1;
    public int CompareTo({{Name}} other) => Value.CompareTo(other.Value);
    public static bool operator <({{Name}} left, {{Name}} right) => left.CompareTo(right) < 0;
    public static bool operator <=({{Name}} left, {{Name}} right) => left.CompareTo(right) <= 0;
    public static bool operator >({{Name}} left, {{Name}} right) => left.CompareTo(right) > 0;
    public static bool operator >=({{Name}} left, {{Name}} right) => left.CompareTo(right) >= 0;
    public static implicit operator decimal({{Name}} typedId) => typedId.Value;
    public static explicit operator {{Name}}(decimal value) => new(value);
    public decimal AsDecimal() => Value;
    public bool IsValid() => {{ValidationBody}};
    public override string ToString() => Value.ToString(System.Globalization.CultureInfo.InvariantCulture);

    public static bool TryParse(string? s, out {{Name}} result)
    {
        if (decimal.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var value))
        {
            result = new {{Name}}(value);
            return true;
        }
        result = default;
        return false;
    }

    public static bool TryParse(string? s, IFormatProvider? provider, out {{Name}} result)
        => TryParse(s, out result);

#if NET7_0_OR_GREATER
    public static {{Name}} Parse(string s, IFormatProvider? provider) =>
        TryParse(s, provider, out var result) ? result : throw new FormatException($""Cannot parse '{s}' as {{Name}}."");
#endif
}";

    public static string GenerateRecordStruct(ValueTypeInfo info)
    {
        var namespaceDecl = string.IsNullOrEmpty(info.Namespace) ? "" : $"namespace {info.Namespace};\n";

        string validationBody;
        if (!info.DecimalMinimum.HasValue && !info.DecimalMaximum.HasValue)
        {
            validationBody = "true";
        }
        else
        {
            var parts = new List<string>();
            if (info.DecimalMinimum.HasValue) parts.Add($"Value >= {info.DecimalMinimum.Value.ToString(CultureInfo.InvariantCulture)}m");
            if (info.DecimalMaximum.HasValue) parts.Add($"Value <= {info.DecimalMaximum.Value.ToString(CultureInfo.InvariantCulture)}m");
            validationBody = string.Join(" && ", parts);
        }

        return TemplateEngine.Render(RecordStructTemplate, new Dictionary<string, string>
        {
            ["Name"] = info.Name,
            ["NamespaceDeclaration"] = namespaceDecl,
            ["ValidationBody"] = validationBody
        });
    }
}
