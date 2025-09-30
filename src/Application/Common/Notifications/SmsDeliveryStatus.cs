namespace EazyMenu.Application.Common.Notifications;

/// <summary>
/// وضعیت نهایی ارسال پیامک تراکنشی.
/// </summary>
public enum SmsDeliveryStatus
{
    Pending = 0,
    Sent = 1,
    Failed = 2
}
