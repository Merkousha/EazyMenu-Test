using System;

namespace EazyMenu.Domain.ValueObjects;

/// <summary>
/// شناسه یکتای کاربر سیستم.
/// </summary>
public readonly record struct UserId(Guid Value)
{
    public static UserId New() => new(Guid.NewGuid());
    
    public static UserId From(Guid id) => new(id);
    
    public override string ToString() => Value.ToString();
}
