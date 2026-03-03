using System.Collections.Generic;

namespace NForza.Wolverine.Generators.CodeGeneration;

internal static class JsonConverterTemplates
{
    private const string GuidTemplate = @"#nullable enable
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

{{NamespaceDeclaration}}
public class {{Name}}JsonConverter : JsonConverter<{{Name}}>
{
    public override {{Name}} Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return default;

        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException($""Expected string, found {reader.TokenType}."");

        string? raw = reader.GetString();
        if (!Guid.TryParse(raw, out Guid guid))
            throw new JsonException($""'{raw}' is not a valid GUID for type {{Name}}."");

        return new {{Name}}(guid);
    }

    public override void Write(Utf8JsonWriter writer, {{Name}} value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value.ToString(""D""));
    }
}";

    private const string StringTemplate = @"#nullable enable
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

{{NamespaceDeclaration}}
public class {{Name}}JsonConverter : JsonConverter<{{Name}}>
{
    public override {{Name}} Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return default;

        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException($""Expected string, found {reader.TokenType}."");

        string? raw = reader.GetString();
        return new {{Name}}(raw ?? string.Empty);
    }

    public override void Write(Utf8JsonWriter writer, {{Name}} value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}";

    private const string IntTemplate = @"#nullable enable
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

{{NamespaceDeclaration}}
public class {{Name}}JsonConverter : JsonConverter<{{Name}}>
{
    public override {{Name}} Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return default;

        int number;

        if (reader.TokenType == JsonTokenType.Number)
        {
            if (!reader.TryGetInt32(out number))
                throw new JsonException($""Number is outside Int32 range for {{Name}}."");
        }
        else if (reader.TokenType == JsonTokenType.String)
        {
            var raw = reader.GetString();
            if (!int.TryParse(raw, out number))
                throw new JsonException($""'{raw}' is not a valid integer for {{Name}}."");
        }
        else
        {
            throw new JsonException($""Expected number or numeric string, found {reader.TokenType}."");
        }

        return new {{Name}}(number);
    }

    public override void Write(Utf8JsonWriter writer, {{Name}} value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.Value);
    }
}";

    private const string DoubleTemplate = @"#nullable enable
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

{{NamespaceDeclaration}}
public class {{Name}}JsonConverter : JsonConverter<{{Name}}>
{
    public override {{Name}} Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return default;

        double number;

        if (reader.TokenType == JsonTokenType.Number)
        {
            if (!reader.TryGetDouble(out number))
                throw new JsonException($""Number cannot be read as double for {{Name}}."");
        }
        else if (reader.TokenType == JsonTokenType.String)
        {
            var raw = reader.GetString();
            if (!double.TryParse(raw, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out number))
                throw new JsonException($""'{raw}' is not a valid double for {{Name}}."");
        }
        else
        {
            throw new JsonException($""Expected number or numeric string, found {reader.TokenType}."");
        }

        return new {{Name}}(number);
    }

    public override void Write(Utf8JsonWriter writer, {{Name}} value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.Value);
    }
}";

    public static string GenerateGuid(ValueTypeInfo info)
    {
        return TemplateEngine.Render(GuidTemplate, CreateValues(info));
    }

    public static string GenerateString(ValueTypeInfo info)
    {
        return TemplateEngine.Render(StringTemplate, CreateValues(info));
    }

    public static string GenerateInt(ValueTypeInfo info)
    {
        return TemplateEngine.Render(IntTemplate, CreateValues(info));
    }

    public static string GenerateDouble(ValueTypeInfo info)
    {
        return TemplateEngine.Render(DoubleTemplate, CreateValues(info));
    }

    private static Dictionary<string, string> CreateValues(ValueTypeInfo info)
    {
        var namespaceDecl = string.IsNullOrEmpty(info.Namespace) ? "" : $"namespace {info.Namespace};\n";
        return new Dictionary<string, string>
        {
            ["Name"] = info.Name,
            ["NamespaceDeclaration"] = namespaceDecl
        };
    }
}
