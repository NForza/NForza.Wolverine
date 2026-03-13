using System;

namespace NForza.Wolverine.ValueTypes;

[AttributeUsage(AttributeTargets.Struct)]
public class DecimalValueAttribute : Attribute
{
    public DecimalValueAttribute(double minimum, double maximum)
    {
        Minimum = (decimal)minimum;
        Maximum = (decimal)maximum;
        HasConstraints = true;
    }

    public DecimalValueAttribute()
    {
        HasConstraints = false;
    }

    public decimal Minimum { get; }
    public decimal Maximum { get; }
    public bool HasConstraints { get; }
}
