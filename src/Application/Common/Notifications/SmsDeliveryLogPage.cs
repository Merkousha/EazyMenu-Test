using System.Collections.Generic;

namespace EazyMenu.Application.Common.Notifications;

/// <summary>
/// نتیجه صفحه‌بندی شده لاگ‌های پیامک برای استفاده در داشبورد.
/// </summary>
public sealed record SmsDeliveryLogPage(
    IReadOnlyCollection<SmsDeliveryRecord> Items,
    bool HasMore);
