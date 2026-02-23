using System.Collections.Generic;
using System.Globalization;

namespace NForza.Wolverine.ValueTypes.Generators.CodeGeneration;

internal static class DoubleValueTemplates
{
    private const string RecordStructTemplate = @"#nullable enable
using System;
using System.Diagnostics;
using System.Text.Json.Serialization;
using NForza.Wolverine.ValueTypes;

{{NamespaceDeclaration}}
[JsonConverter(typeof({{Name}}JsonConverter))]
[DebuggerDisplay(""{Value}"")]
public partial record struct {{Name}}(double Value) : IDoubleValueType, IComparable, IComparable<{{Name}}>, IEquatable<{{Name}}>
{
    public int CompareTo(object? other) => other is {{Name}} ? Value.CompareTo((({{Name}})other).Value) : -1;
    public int CompareTo({{Name}} other) => Value.CompareTo(other.Value);
    public static bool operator <({{Name}} left, {{Name}} right) => left.CompareTo(right) < 0;
    public static bool operator <=({{Name}} left, {{Name}} right) => left.CompareTo(right) <= 0;
    public static bool operator >({{Name}} left, {{Name}} right) => left.CompareTo(right) > 0;
    public static bool operator >=({{Name}} left, {{Name}} right) => left.CompareTo(right) >= 0;
    public static implicit operator double({{Name}} typedId) => typedId.Value;
    public static explicit operator {{Name}}(double value) => new(value);
    public double AsDouble() => Value;
    public bool IsValid() => {{ValidationBody}};
    public override string ToString() => Value.ToString();

    public static bool TryParse(string? s, out {{Name}} result)
    {
        if (double.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var value))
        {
            result = new {{Name}}(value);
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

        string validationBody;
        if (!info.DoubleMinimum.HasValue && !info.DoubleMaximum.HasValue)
        {
            validationBody = "true";
        }
        else
        {
            var parts = new System.Collections.Generic.List<string>();
            if (info.DoubleMinimum.HasValue) parts.Add($"Value >= {info.DoubleMinimum.Value.ToString(CultureInfo.InvariantCulture)}");
            if (info.DoubleMaximum.HasValue) parts.Add($"Value <= {info.DoubleMaximum.Value.ToString(CultureInfo.InvariantCulture)}");
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
