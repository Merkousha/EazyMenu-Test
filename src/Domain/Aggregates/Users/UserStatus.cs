namespace EazyMenu.Domain.Aggregates.Users;

/// <summary>
/// وضعیت کاربر در سیستم.
/// </summary>
public enum UserStatus
{
    /// <summary>
    /// کاربر فعال و می‌تواند وارد سیستم شود.
    /// </summary>
    Active = 1,
    
    /// <summary>
    /// کاربر غیرفعال شده و نمی‌تواند وارد سیستم شود.
    /// </summary>
    Inactive = 2,
    
    /// <summary>
    /// کاربر مسدود شده به دلیل نقض قوانین.
    /// </summary>
    Blocked = 3
}
