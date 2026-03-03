using System.Collections.Generic;

namespace NForza.Wolverine.Generators.CodeGeneration;

internal static class TemplateEngine
{
    public static string Render(string template, Dictionary<string, string> values)
    {
        var result = template;
        foreach (var kvp in values)
        {
            result = result.Replace("{{" + kvp.Key + "}}", kvp.Value);
        }
        return result;
    }
}
