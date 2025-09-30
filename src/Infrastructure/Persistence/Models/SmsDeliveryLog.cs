using System;
using EazyMenu.Application.Common.Notifications;

namespace EazyMenu.Infrastructure.Persistence.Models;

/// <summary>
/// رکورد ذخیره وضعیت ارسال پیامک برای گزارش‌گیری مدیریتی.
/// </summary>
public sealed class SmsDeliveryLog
{
    public Guid Id { get; set; }

    public string PhoneNumber { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string Provider { get; set; } = string.Empty;

    public SmsDeliveryStatus Status { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    public string? ErrorCode { get; set; }

    public string? ErrorMessage { get; set; }

    public string? Payload { get; set; }
}
