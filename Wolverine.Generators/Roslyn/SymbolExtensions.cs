using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace NForza.Wolverine.Generators.Roslyn;

internal static class SymbolExtensions
{
    public static string GetNameOrEmpty(this INamespaceSymbol symbol)
    {
        return symbol.IsGlobalNamespace ? string.Empty : symbol.ToDisplayString();
    }

    public static bool HasAttribute(this ISymbol typeSymbol, string attributeName)
        => typeSymbol
            .GetAttributes()
            .Select(a => a.AttributeClass?.Name ?? "")
            .Any(name => name == attributeName || name == attributeName + "Attribute");

    public static string GetUnderlyingTypeOfValueType(this INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.GetAttributes()
            .Select(a => a.AttributeClass?.Name)
            .FirstOrDefault(name => name is "StringValueAttribute" or "IntValueAttribute" or "DoubleValueAttribute" or "GuidValueAttribute") switch
        {
            "StringValueAttribute" => "string",
            "IntValueAttribute" => "int",
            "DoubleValueAttribute" => "double",
            "GuidValueAttribute" => "System.Guid",
            _ => throw new InvalidOperationException($"Unknown value type for {typeSymbol.Name}.")
        };
    }
}
