using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Common.Notifications;

namespace EazyMenu.Application.Features.Notifications.GetSmsDeliveryLogs;

/// <summary>
/// درخواست بازیابی لاگ‌های ارسال پیامک برای داشبورد مدیریتی.
/// </summary>
public sealed record GetSmsDeliveryLogsQuery(
    int Page,
    int PageSize,
    SmsDeliveryStatus? Status) : IQuery<SmsDeliveryLogPage>;
