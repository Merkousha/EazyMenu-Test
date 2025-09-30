using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Common.Interfaces;
using EazyMenu.Application.Common.Interfaces.Notifications;
using EazyMenu.Application.Common.Notifications;
using Microsoft.Extensions.Logging;

namespace EazyMenu.Infrastructure.Notifications;

internal sealed class KavenegarSmsSender : ISmsSender
{
    private readonly HttpClient _httpClient;
    private readonly SmsOptions _options;
    private readonly ILogger<KavenegarSmsSender> _logger;
    private readonly ISmsDeliveryStore _deliveryStore;
    private readonly IDateTimeProvider _dateTimeProvider;

    public KavenegarSmsSender(
        HttpClient httpClient,
        SmsOptions options,
        ILogger<KavenegarSmsSender> logger,
        ISmsDeliveryStore deliveryStore,
        IDateTimeProvider dateTimeProvider)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;
        _deliveryStore = deliveryStore;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task SendAsync(string phoneNumber, string message, SmsSendContext? context = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            throw new ArgumentException("شماره موبایل خالی است.", nameof(phoneNumber));
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("متن پیام خالی است.", nameof(message));
        }

        if (string.IsNullOrWhiteSpace(_options.KavenegarApiKey))
        {
            await RecordAsync(SmsDeliveryStatus.Failed, phoneNumber, message, "configuration", "کلید API تنظیم نشده است.", null, context, cancellationToken);
            throw new InvalidOperationException("کلید API کاوه‌نگار پیکربندی نشده است.");
        }

        var requestUri = new Uri($"v1/{_options.KavenegarApiKey}/sms/send.json", UriKind.Relative);
        var requestBody = new Dictionary<string, string>
        {
            ["receptor"] = phoneNumber,
            ["message"] = message
        };

        if (!string.IsNullOrWhiteSpace(_options.KavenegarSenderLine))
        {
            requestBody["sender"] = _options.KavenegarSenderLine!;
        }

        using var content = new FormUrlEncodedContent(requestBody);
        try
        {
            var response = await _httpClient.PostAsync(requestUri, content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("پیامک کاوه‌نگار برای {PhoneNumber} با موفقیت ارسال شد.", phoneNumber);
                await RecordAsync(SmsDeliveryStatus.Sent, phoneNumber, message, null, null, null, context, cancellationToken);
                return;
            }

            var payload = await SafeReadAsync(response, cancellationToken);
            var errorCode = ((int)response.StatusCode).ToString();
            _logger.LogError("ارسال پیامک کاوه‌نگار برای {PhoneNumber} با خطا مواجه شد. StatusCode: {StatusCode}, Response: {Payload}", phoneNumber, (int)response.StatusCode, payload);
            await RecordAsync(SmsDeliveryStatus.Failed, phoneNumber, message, errorCode, "پاسخ ناموفق از سرویس کاوه‌نگار", payload, context, cancellationToken);
            throw new InvalidOperationException("ارسال پیامک کاوه‌نگار با شکست مواجه شد.");
        }
        catch (HttpRequestException exception)
        {
            await RecordAsync(SmsDeliveryStatus.Failed, phoneNumber, message, "network", exception.Message, null, context, cancellationToken);
            _logger.LogError(exception, "ارسال پیامک کاوه‌نگار برای {PhoneNumber} با خطای شبکه مواجه شد.", phoneNumber);
            throw;
        }
    }

    private static async Task<string> SafeReadAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch
        {
            return "<no-content>";
        }
    }

    private async Task RecordAsync(
        SmsDeliveryStatus status,
        string phoneNumber,
        string message,
        string? errorCode,
    string? errorMessage,
    string? payload,
    SmsSendContext? context = null,
    CancellationToken cancellationToken = default)
    {
        var record = new SmsDeliveryRecord(
            Guid.NewGuid(),
            phoneNumber,
            message,
            nameof(SmsProvider.Kavenegar),
            status,
            new DateTimeOffset(_dateTimeProvider.UtcNow),
            errorCode,
            errorMessage,
            payload,
            context?.TenantId,
            context?.SubscriptionPlan);

        try
        {
            await _deliveryStore.RecordAsync(record, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "ثبت نتیجه ارسال پیامک کاوه‌نگار با شکست مواجه شد.");
        }
    }
}
