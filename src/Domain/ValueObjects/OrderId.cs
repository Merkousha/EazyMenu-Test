using System;
using System.Diagnostics.CodeAnalysis;

namespace EazyMenu.Domain.ValueObjects;

public readonly record struct OrderId
{
    public Guid Value { get; }

    private OrderId(Guid value)
    {
        Value = value;
    }

    public static OrderId New() => new(Guid.NewGuid());

    public static OrderId FromGuid(Guid value)
    {
        if (!TryCreate(value, out var id))
        {
            throw new ArgumentException("شناسه سفارش معتبر نیست.", nameof(value));
        }

        return id;
    }

    public static bool TryCreate(Guid value, [NotNullWhen(true)] out OrderId id)
    {
        id = default;
        if (value == Guid.Empty)
        {
            return false;
        }

        id = new OrderId(value);
        return true;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(OrderId id) => id.Value;
}
