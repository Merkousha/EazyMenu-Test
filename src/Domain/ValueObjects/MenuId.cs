using System;
using System.Diagnostics.CodeAnalysis;

namespace EazyMenu.Domain.ValueObjects;

public readonly record struct MenuId
{
    public Guid Value { get; }

    private MenuId(Guid value)
    {
        Value = value;
    }

    public static MenuId New() => new(Guid.NewGuid());

    public static MenuId FromGuid(Guid value)
    {
        if (!TryCreate(value, out var id))
        {
            throw new ArgumentException("شناسه منو معتبر نیست.", nameof(value));
        }

        return id;
    }

    public static bool TryCreate(Guid value, [NotNullWhen(true)] out MenuId id)
    {
        id = default;
        if (value == Guid.Empty)
        {
            return false;
        }

        id = new MenuId(value);
        return true;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(MenuId id) => id.Value;
}
