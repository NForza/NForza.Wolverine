using System;

namespace NForza.Wolverine.ValueTypes;

[AttributeUsage(AttributeTargets.Struct)]
public class LongValueAttribute : Attribute
{
    public LongValueAttribute(long minimum, long maximum)
    {
        Minimum = minimum;
        Maximum = maximum;
    }

    public LongValueAttribute() : this(long.MinValue, long.MaxValue)
    {
    }

    public long Minimum { get; }
    public long Maximum { get; }
}
