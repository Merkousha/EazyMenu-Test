namespace EazyMenu.Domain.Aggregates.Users;

/// <summary>
/// نقش‌های کاربران در سیستم.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// مالک رستوران - دسترسی کامل به تمام بخش‌ها.
    /// </summary>
    Owner = 1,
    
    /// <summary>
    /// مدیر رستوران - دسترسی به اکثر بخش‌ها به جز تنظیمات حساس.
    /// </summary>
    Manager = 2,
    
    /// <summary>
    /// کارمند - دسترسی محدود به مدیریت سفارش‌ها و منو.
    /// </summary>
    Staff = 3
}
