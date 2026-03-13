using System.Collections.Generic;

namespace NForza.Wolverine.Generators.CodeGeneration;

internal static class DateOnlyValueTemplates
{
    private const string RecordStructTemplate = @"#nullable enable
using System;
using System.Diagnostics;
using System.Text.Json.Serialization;
using NForza.Wolverine.ValueTypes;

{{NamespaceDeclaration}}
[JsonConverter(typeof({{Name}}JsonConverter))]
[DebuggerDisplay(""{Value}"")]
public partial record struct {{Name}}(DateOnly Value) : IDateOnlyValueType, IComparable, IComparable<{{Name}}>, IEquatable<{{Name}}>
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
    public static explicit operator DateOnly({{Name}} typedId) => typedId.Value;
    public static explicit operator {{Name}}(DateOnly value) => new(value);
    public DateOnly AsDateOnly() => Value;
    public bool IsValid() => Value != default;
    public override string ToString() => Value.ToString(""yyyy-MM-dd"");

    public static bool TryParse(string? s, out {{Name}} result)
    {
        if (DateOnly.TryParse(s, out var value))
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
        return TemplateEngine.Render(RecordStructTemplate, new Dictionary<string, string>
        {
            ["Name"] = info.Name,
            ["NamespaceDeclaration"] = namespaceDecl
        });
    }
}
