using System;
using System.Diagnostics.CodeAnalysis;

namespace EazyMenu.Domain.ValueObjects;

public readonly record struct PaymentId
{
    public Guid Value { get; }

    private PaymentId(Guid value)
    {
        Value = value;
    }

    public static PaymentId New() => new(Guid.NewGuid());

    public static bool TryCreate(Guid value, [NotNullWhen(true)] out PaymentId paymentId)
    {
        paymentId = default;
        if (value == Guid.Empty)
        {
            return false;
        }

        paymentId = new PaymentId(value);
        return true;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(PaymentId id) => id.Value;
}
