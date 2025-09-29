using System;
using System.Diagnostics.CodeAnalysis;

namespace EazyMenu.Domain.ValueObjects;

public readonly record struct BranchId
{
    public Guid Value { get; }

    private BranchId(Guid value)
    {
        Value = value;
    }

    public static BranchId New() => new(Guid.NewGuid());

    public static bool TryCreate(Guid value, [NotNullWhen(true)] out BranchId branchId)
    {
        branchId = default;
        if (value == Guid.Empty)
        {
            return false;
        }

        branchId = new BranchId(value);
        return true;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(BranchId id) => id.Value;
}
