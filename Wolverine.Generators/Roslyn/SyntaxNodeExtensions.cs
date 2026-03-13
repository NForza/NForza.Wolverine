using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NForza.Wolverine.Generators.Roslyn;

internal static class SyntaxNodeExtensions
{
    public static bool IsRecordWithAttribute(this SyntaxNode syntaxNode, string attributeName)
    {
        return syntaxNode is RecordDeclarationSyntax recordDeclaration &&
               recordDeclaration.AttributeLists
                   .SelectMany(al => al.Attributes)
                   .Any(a => a.Name.ToString() == attributeName || a.Name.ToString() == attributeName + "Attribute");
    }

    public static bool IsRecordWithGuidValueAttribute(this SyntaxNode syntaxNode)
        => IsRecordWithAttribute(syntaxNode, "GuidValue");

    public static bool IsRecordWithIntValueAttribute(this SyntaxNode syntaxNode)
        => IsRecordWithAttribute(syntaxNode, "IntValue");

    public static bool IsRecordWithDoubleValueAttribute(this SyntaxNode syntaxNode)
        => IsRecordWithAttribute(syntaxNode, "DoubleValue");

    public static bool IsRecordWithStringValueAttribute(this SyntaxNode syntaxNode)
        => IsRecordWithAttribute(syntaxNode, "StringValue");

    public static bool IsRecordWithLongValueAttribute(this SyntaxNode syntaxNode)
        => IsRecordWithAttribute(syntaxNode, "LongValue");

    public static bool IsRecordWithDecimalValueAttribute(this SyntaxNode syntaxNode)
        => IsRecordWithAttribute(syntaxNode, "DecimalValue");

    public static bool IsRecordWithDateOnlyValueAttribute(this SyntaxNode syntaxNode)
        => IsRecordWithAttribute(syntaxNode, "DateOnlyValue");

    public static bool IsRecordWithDateTimeValueAttribute(this SyntaxNode syntaxNode)
        => IsRecordWithAttribute(syntaxNode, "DateTimeValue");

    public static bool IsRecordWithDateTimeOffsetValueAttribute(this SyntaxNode syntaxNode)
        => IsRecordWithAttribute(syntaxNode, "DateTimeOffsetValue");

    public static bool IsClassInheritingFromWolverineHub(this SyntaxNode syntaxNode)
    {
        return syntaxNode is ClassDeclarationSyntax classDeclaration &&
               classDeclaration.BaseList?.Types
                   .Any(t => t.Type.ToString() == "WolverineHub") == true;
    }
}
