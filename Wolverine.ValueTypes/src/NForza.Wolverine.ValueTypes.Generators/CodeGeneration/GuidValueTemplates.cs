using System.Collections.Generic;

namespace NForza.Wolverine.ValueTypes.Generators.CodeGeneration;

internal static class GuidValueTemplates
{
    private const string RecordStructTemplate = @"#nullable enable
using System;
using System.Diagnostics;
using System.Text.Json.Serialization;
using NForza.Wolverine.ValueTypes;

{{NamespaceDeclaration}}
[JsonConverter(typeof({{Name}}JsonConverter))]
[DebuggerDisplay(""{Value}"")]
public partial record struct {{Name}}(Guid Value) : IGuidValueType, IComparable<{{Name}}>, IComparable
{
    public {{Name}}() : this(Guid.NewGuid()) { }
    public {{Name}}(string guid) : this(Guid.Parse(guid)) { }
    public static readonly {{Name}} Empty = new {{Name}}(Guid.Empty);
    public static explicit operator Guid({{Name}} typedId) => typedId.Value;
    public static explicit operator {{Name}}(Guid value) => new(value);
    public Guid AsGuid() => Value;
    public override string ToString() => Value.ToString();
    public bool IsValid() => !Value.Equals(Guid.Empty);
    public int CompareTo({{Name}} other) => Value.CompareTo(other.Value);
    public int CompareTo(object? obj) => obj is {{Name}} other ? CompareTo(other) : throw new ArgumentException(""Object is not a {{Name}}"");

    public static bool TryParse(string? s, out {{Name}} result)
    {
        if (Guid.TryParse(s, out var guid))
        {
            result = new {{Name}}(guid);
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
        return TemplateEngine.Render(RecordStructTemplate, new Dictionary<string, string>
        {
            ["Name"] = info.Name,
            ["NamespaceDeclaration"] = namespaceDecl
        });
    }
}
