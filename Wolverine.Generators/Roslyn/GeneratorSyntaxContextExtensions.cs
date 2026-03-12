using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NForza.Wolverine.Generators.Roslyn;

internal static class GeneratorSyntaxContextExtensions
{
    public static INamedTypeSymbol? GetRecordSymbolFromContext(this GeneratorSyntaxContext context)
    {
        var recordDeclaration = (RecordDeclarationSyntax)context.Node;
        var model = context.SemanticModel;
        return model.GetDeclaredSymbol(recordDeclaration) as INamedTypeSymbol;
    }
}
