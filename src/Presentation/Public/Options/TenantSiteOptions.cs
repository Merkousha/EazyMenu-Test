using System;

namespace EazyMenu.Public.Options;

/// <summary>
/// تنظیمات مربوط به سایت عمومی برای نگهداری شناسه مستاجر پیش‌فرض.
/// </summary>
public sealed class TenantSiteOptions
{
    /// <summary>
    /// شناسه مستاجر پیش‌فرض برای مواقعی که در URL مشخص نشده باشد.
    /// </summary>
    public Guid DefaultTenantId { get; set; } = Guid.Parse("11111111-1111-1111-1111-111111111111");
}
