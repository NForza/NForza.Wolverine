namespace NForza.Wolverine.Generators;

internal enum ValueTypeKind
{
    Guid,
    String,
    Int,
    Double,
    Long,
    Decimal,
    DateOnly,
    DateTime,
    DateTimeOffset
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
        ValueTypeKind.Long => "long",
        ValueTypeKind.Decimal => "decimal",
        ValueTypeKind.DateOnly => "System.DateOnly",
        ValueTypeKind.DateTime => "System.DateTime",
        ValueTypeKind.DateTimeOffset => "System.DateTimeOffset",
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

    // Long validation
    public long? LongMinimum { get; set; }
    public long? LongMaximum { get; set; }

    // Decimal validation
    public decimal? DecimalMinimum { get; set; }
    public decimal? DecimalMaximum { get; set; }
}
