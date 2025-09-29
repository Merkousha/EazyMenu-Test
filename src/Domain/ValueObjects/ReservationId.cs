using System;
using System.Diagnostics.CodeAnalysis;

namespace EazyMenu.Domain.ValueObjects;

public readonly record struct ReservationId
{
    public Guid Value { get; }

    private ReservationId(Guid value)
    {
        Value = value;
    }

    public static ReservationId New() => new(Guid.NewGuid());

    public static bool TryCreate(Guid value, [NotNullWhen(true)] out ReservationId reservationId)
    {
        reservationId = default;
        if (value == Guid.Empty)
        {
            return false;
        }

        reservationId = new ReservationId(value);
        return true;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(ReservationId id) => id.Value;
}
