using System.Collections.Generic;

namespace NForza.Wolverine.ValueTypes.Generators.CodeGeneration;

internal static class IntValueTemplates
{
    private const string RecordStructTemplate = @"#nullable enable
using System;
using System.Diagnostics;
using System.Text.Json.Serialization;
using NForza.Wolverine.ValueTypes;

{{NamespaceDeclaration}}
[JsonConverter(typeof({{Name}}JsonConverter))]
[DebuggerDisplay(""{Value}"")]
public partial record struct {{Name}}(int Value) : IIntValueType, IComparable, IComparable<{{Name}}>, IEquatable<{{Name}}>
{
    public int CompareTo(object? other) => other is {{Name}} ? Value.CompareTo((({{Name}})other).Value) : -1;
    public int CompareTo({{Name}} other) => Value.CompareTo(other.Value);
    public static bool operator <({{Name}} left, {{Name}} right) => left.CompareTo(right) < 0;
    public static bool operator <=({{Name}} left, {{Name}} right) => left.CompareTo(right) <= 0;
    public static bool operator >({{Name}} left, {{Name}} right) => left.CompareTo(right) > 0;
    public static bool operator >=({{Name}} left, {{Name}} right) => left.CompareTo(right) >= 0;
    public static implicit operator int({{Name}} typedId) => typedId.Value;
    public static explicit operator {{Name}}(int value) => new(value);
    public int AsInt() => Value;
    public bool IsValid() => {{ValidationBody}};
    public override string ToString() => Value.ToString();

    public static bool TryParse(string? s, out {{Name}} result)
    {
        if (int.TryParse(s, out var value))
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
        if (!info.IntMinimum.HasValue && !info.IntMaximum.HasValue)
        {
            validationBody = "true";
        }
        else
        {
            var parts = new List<string>();
            if (info.IntMinimum.HasValue) parts.Add($"Value >= {info.IntMinimum.Value}");
            if (info.IntMaximum.HasValue) parts.Add($"Value <= {info.IntMaximum.Value}");
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
