using System;
using System.Diagnostics.CodeAnalysis;

namespace EazyMenu.Domain.ValueObjects;

public readonly record struct TableId
{
    public Guid Value { get; }

    private TableId(Guid value)
    {
        Value = value;
    }

    public static TableId New() => new(Guid.NewGuid());

    public static bool TryCreate(Guid value, [NotNullWhen(true)] out TableId tableId)
    {
        tableId = default;
        if (value == Guid.Empty)
        {
            return false;
        }

        tableId = new TableId(value);
        return true;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(TableId id) => id.Value;
}
