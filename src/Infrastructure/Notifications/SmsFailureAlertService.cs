using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Common.Interfaces;
using EazyMenu.Application.Common.Interfaces.Notifications;
using EazyMenu.Application.Common.Notifications;
using Microsoft.Extensions.Logging;

namespace EazyMenu.Infrastructure.Notifications;

internal sealed class SmsFailureAlertService : ISmsFailureAlertService
{
    private readonly IEmailSender _emailSender;
    private readonly ISmsDeliveryStore _smsDeliveryStore;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly EmailOptions _emailOptions;
    private readonly ISmsFailureAlertNotifier _alertNotifier;
    private readonly ILogger<SmsFailureAlertService> _logger;

    public SmsFailureAlertService(
        IEmailSender emailSender,
        ISmsDeliveryStore smsDeliveryStore,
        IDateTimeProvider dateTimeProvider,
        EmailOptions emailOptions,
        ISmsFailureAlertNotifier alertNotifier,
        ILogger<SmsFailureAlertService> logger)
    {
        _emailSender = emailSender;
        _smsDeliveryStore = smsDeliveryStore;
        _dateTimeProvider = dateTimeProvider;
        _emailOptions = emailOptions;
        _alertNotifier = alertNotifier;
        _logger = logger;
    }

    public async Task NotifyFailureAsync(string phoneNumber, string message, Exception exception, CancellationToken cancellationToken = default)
    {
        var occurredAt = new DateTimeOffset(_dateTimeProvider.UtcNow);
        var subject = "هشدار: ارسال پیامک ناموفق";
        var bodyBuilder = new StringBuilder()
            .AppendLine("ارسال پیامک تراکنشی با خطا مواجه شد.")
            .AppendLine($"گیرنده: {phoneNumber}")
            .AppendLine($"متن: {message}")
            .AppendLine($"زمان: {occurredAt:yyyy/MM/dd HH:mm:ss}")
            .AppendLine($"خطا: {exception.GetType().Name} - {exception.Message}");

        try
        {
            await _emailSender.SendAsync(_emailOptions.SupportAddress, subject, bodyBuilder.ToString(), cancellationToken);
            await RecordFallbackAsync(phoneNumber, message, occurredAt, SmsDeliveryStatus.Sent, null, null, cancellationToken);
        }
        catch (Exception emailException)
        {
            _logger.LogError(emailException, "ارسال ایمیل هشدار پیامک ناموفق بود.");
            await RecordFallbackAsync(phoneNumber, message, occurredAt, SmsDeliveryStatus.Failed, "email", emailException.Message, cancellationToken);
        }

        var alert = new SmsFailureAlert(
            phoneNumber,
            message,
            occurredAt,
            exception.Message,
            "email");

        try
        {
            await _alertNotifier.PublishAsync(alert, cancellationToken);
        }
        catch (Exception publishException)
        {
            _logger.LogError(publishException, "ارسال اعلان real-time برای شکست پیامک با خطا مواجه شد.");
        }
    }

    private async Task RecordFallbackAsync(
        string phoneNumber,
        string message,
        DateTimeOffset occurredAt,
        SmsDeliveryStatus status,
        string? errorCode,
        string? errorMessage,
        CancellationToken cancellationToken)
    {
        var record = new SmsDeliveryRecord(
            Guid.NewGuid(),
            phoneNumber,
            message,
            "fallback-email",
            status,
            occurredAt,
            errorCode,
            errorMessage,
            null);

        try
        {
            await _smsDeliveryStore.RecordAsync(record, cancellationToken);
        }
        catch (Exception storeException)
        {
            _logger.LogError(storeException, "ثبت لاگ fallback ایمیل با شکست مواجه شد.");
        }
    }
}
