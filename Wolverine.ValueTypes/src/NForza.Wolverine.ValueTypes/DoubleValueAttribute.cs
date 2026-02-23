using System;

namespace NForza.Wolverine.ValueTypes;

[AttributeUsage(AttributeTargets.Struct)]
public class DoubleValueAttribute : Attribute
{
    public DoubleValueAttribute(double minimum, double maximum)
    {
        Minimum = minimum;
        Maximum = maximum;
    }

    public DoubleValueAttribute() : this(double.NaN, double.NaN)
    {
    }

    public double Minimum { get; }
    public double Maximum { get; }
}
