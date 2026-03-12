namespace NForza.Wolverine.Generators;

internal enum ValueTypeKind
{
    Guid,
    String,
    Int,
    Double
}

internal class ValueTypeInfo
{
    public string Name { get; set; } = "";
    public string Namespace { get; set; } = "";
    public string FullyQualifiedName => string.IsNullOrEmpty(Namespace) ? Name : Namespace + "." + Name;
    public ValueTypeKind Kind { get; set; }
    public string UnderlyingType => Kind switch
    {
        ValueTypeKind.Guid => "System.Guid",
        ValueTypeKind.String => "string",
        ValueTypeKind.Int => "int",
        ValueTypeKind.Double => "double",
        _ => "object"
    };

    // String validation
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public string? ValidationRegex { get; set; }

    // Int validation
    public int? IntMinimum { get; set; }
    public int? IntMaximum { get; set; }

    // Double validation
    public double? DoubleMinimum { get; set; }
    public double? DoubleMaximum { get; set; }
}
