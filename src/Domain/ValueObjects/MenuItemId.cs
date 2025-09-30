using System;
using System.Diagnostics.CodeAnalysis;

namespace EazyMenu.Domain.ValueObjects;

public readonly record struct MenuItemId
{
    public Guid Value { get; }

    private MenuItemId(Guid value)
    {
        Value = value;
    }

    public static MenuItemId New() => new(Guid.NewGuid());

    public static MenuItemId FromGuid(Guid value)
    {
        if (!TryCreate(value, out var id))
        {
            throw new ArgumentException("شناسه آیتم منو معتبر نیست.", nameof(value));
        }

        return id;
    }

    public static bool TryCreate(Guid value, [NotNullWhen(true)] out MenuItemId id)
    {
        id = default;
        if (value == Guid.Empty)
        {
            return false;
        }

        id = new MenuItemId(value);
        return true;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(MenuItemId id) => id.Value;
}
