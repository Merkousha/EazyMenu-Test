using System;
using System.Diagnostics.CodeAnalysis;

namespace EazyMenu.Domain.ValueObjects;

public readonly record struct MenuCategoryId
{
    public Guid Value { get; }

    private MenuCategoryId(Guid value)
    {
        Value = value;
    }

    public static MenuCategoryId New() => new(Guid.NewGuid());

    public static MenuCategoryId FromGuid(Guid value)
    {
        if (!TryCreate(value, out var id))
        {
            throw new ArgumentException("شناسه دسته منو معتبر نیست.", nameof(value));
        }

        return id;
    }

    public static bool TryCreate(Guid value, [NotNullWhen(true)] out MenuCategoryId id)
    {
        id = default;
        if (value == Guid.Empty)
        {
            return false;
        }

        id = new MenuCategoryId(value);
        return true;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(MenuCategoryId id) => id.Value;
}
