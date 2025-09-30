using System;
using EazyMenu.Domain.Aggregates.Tenants;

namespace EazyMenu.Application.Common.Notifications;

/// <summary>
/// داده‌های اعلان شکست ارسال پیامک برای نمایش در داشبورد.
/// </summary>
public sealed record SmsFailureAlert(
    string PhoneNumber,
    string Message,
    DateTimeOffset OccurredAt,
    string ErrorMessage,
    string Channel,
    Guid? TenantId = null,
    SubscriptionPlan? SubscriptionPlan = null);
