using System;
using System.Collections.Generic;
using EazyMenu.Domain.Common;
using EazyMenu.Domain.Common.Guards;

namespace EazyMenu.Domain.ValueObjects;

public sealed class Percentage : ValueObject
{
    public decimal Value { get; }

    private Percentage(decimal value)
    {
        if (value < 0 || value > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "درصد باید بین 0 تا 100 باشد.");
        }

        Value = decimal.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    public static Percentage From(decimal value) => new(value);

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

    public decimal ToFraction() => Value / 100m;
}
