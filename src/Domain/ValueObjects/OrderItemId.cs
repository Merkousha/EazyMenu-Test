using System;
using System.Diagnostics.CodeAnalysis;

namespace EazyMenu.Domain.ValueObjects;

public readonly record struct OrderItemId
{
    public Guid Value { get; }

    private OrderItemId(Guid value)
    {
        Value = value;
    }

    public static OrderItemId New() => new(Guid.NewGuid());

    public static OrderItemId FromGuid(Guid value)
    {
        if (!TryCreate(value, out var id))
        {
            throw new ArgumentException("شناسه آیتم سفارش معتبر نیست.", nameof(value));
        }

        return id;
    }

    public static bool TryCreate(Guid value, [NotNullWhen(true)] out OrderItemId id)
    {
        id = default;
        if (value == Guid.Empty)
        {
            return false;
        }

        id = new OrderItemId(value);
        return true;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(OrderItemId id) => id.Value;
}
