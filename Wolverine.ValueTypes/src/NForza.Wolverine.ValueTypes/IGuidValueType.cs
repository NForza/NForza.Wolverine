using System;

namespace NForza.Wolverine.ValueTypes;

public interface IGuidValueType : IValueType
{
    Guid Value { get; }
}
