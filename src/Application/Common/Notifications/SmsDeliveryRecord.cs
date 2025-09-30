using System;

namespace EazyMenu.Application.Common.Notifications;

/// <summary>
/// رکورد ثبت وضعیت ارسال پیامک به همراه متادیتا برای گزارش‌گیری.
/// </summary>
public sealed record SmsDeliveryRecord(
    Guid Id,
    string PhoneNumber,
    string Message,
    string Provider,
    SmsDeliveryStatus Status,
    DateTimeOffset OccurredAt,
    string? ErrorCode = null,
    string? ErrorMessage = null,
    string? Payload = null);
