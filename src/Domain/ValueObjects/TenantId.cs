using System;
using System.Diagnostics.CodeAnalysis;

namespace EazyMenu.Domain.ValueObjects;

public readonly record struct TenantId
{
    public Guid Value { get; }

    private TenantId(Guid value)
    {
        Value = value;
    }

    public static TenantId New() => new(Guid.NewGuid());

    public static TenantId FromGuid(Guid value)
    {
        if (!TryCreate(value, out var id))
        {
            throw new ArgumentException("شناسه مستاجر معتبر نیست.", nameof(value));
        }

        return id;
    }

    public static bool TryCreate(Guid value, [NotNullWhen(true)] out TenantId tenantId)
    {
        tenantId = default;
        if (value == Guid.Empty)
        {
            return false;
        }

        tenantId = new TenantId(value);
        return true;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(TenantId id) => id.Value;
}
