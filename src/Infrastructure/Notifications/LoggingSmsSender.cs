using System;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Common.Interfaces;
using EazyMenu.Application.Common.Interfaces.Notifications;
using EazyMenu.Application.Common.Notifications;
using Microsoft.Extensions.Logging;

namespace EazyMenu.Infrastructure.Notifications;

internal sealed class LoggingSmsSender : ISmsSender
{
    private readonly ILogger<LoggingSmsSender> _logger;
    private readonly ISmsDeliveryStore _deliveryStore;
    private readonly IDateTimeProvider _dateTimeProvider;

    public LoggingSmsSender(
        ILogger<LoggingSmsSender> logger,
        ISmsDeliveryStore deliveryStore,
        IDateTimeProvider dateTimeProvider)
    {
        _logger = logger;
        _deliveryStore = deliveryStore;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task SendAsync(string phoneNumber, string message, SmsSendContext? context = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("ارسال پیامک به {PhoneNumber}: {Message}", phoneNumber, message);

        await TryRecordAsync(new SmsDeliveryRecord(
            Guid.NewGuid(),
            phoneNumber,
            message,
            nameof(SmsProvider.Logging),
            SmsDeliveryStatus.Sent,
            new DateTimeOffset(_dateTimeProvider.UtcNow),
            TenantId: context?.TenantId,
            SubscriptionPlan: context?.SubscriptionPlan), cancellationToken);
    }

    private async Task TryRecordAsync(SmsDeliveryRecord record, CancellationToken cancellationToken)
    {
        try
        {
            await _deliveryStore.RecordAsync(record, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "ثبت نتیجه ارسال پیامک لاگینگ با شکست مواجه شد.");
        }
    }
}
