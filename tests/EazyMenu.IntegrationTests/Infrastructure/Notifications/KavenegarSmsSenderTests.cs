using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Common.Interfaces;
using EazyMenu.Application.Common.Interfaces.Notifications;
using EazyMenu.Application.Common.Notifications;
using EazyMenu.Infrastructure.Notifications;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace EazyMenu.IntegrationTests.Infrastructure.Notifications;

public class KavenegarSmsSenderTests
{
    [Fact]
    public async Task SendAsync_Succeeds_WhenGatewayReturnsOk()
    {
        var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK));
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.kavenegar.com/"),
            Timeout = TimeSpan.FromSeconds(10)
        };
        var options = new SmsOptions
        {
            Provider = nameof(SmsProvider.Kavenegar),
            KavenegarApiKey = "test-api-key",
            KavenegarSenderLine = "10004321"
        };
        var deliveryStore = new FakeSmsDeliveryStore();
        var dateTimeProvider = new FixedDateTimeProvider(new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc));
        var sender = new KavenegarSmsSender(httpClient, options, NullLogger<KavenegarSmsSender>.Instance, deliveryStore, dateTimeProvider);

        await sender.SendAsync("09120000000", "کد ورود شما 1234");

        Assert.NotNull(handler.LastRequest);
        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.Equal(new Uri("https://api.kavenegar.com/v1/test-api-key/sms/send.json"), handler.LastRequest!.RequestUri);
        Assert.Equal("09120000000", handler.LastFormValues["receptor"]);
        Assert.Equal("کد ورود شما 1234", handler.LastFormValues["message"]);
        Assert.Equal("10004321", handler.LastFormValues["sender"]);

        var record = Assert.Single(deliveryStore.Records);
        Assert.Equal(SmsDeliveryStatus.Sent, record.Status);
        Assert.Equal("09120000000", record.PhoneNumber);
        Assert.Equal("کد ورود شما 1234", record.Message);
        Assert.Equal(nameof(SmsProvider.Kavenegar), record.Provider);
        Assert.Equal(dateTimeProvider.UtcNow, record.OccurredAt.UtcDateTime);
    }

    [Fact]
    public async Task SendAsync_Throws_WhenApiKeyMissing()
    {
        using var httpClient = new HttpClient(new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)))
        {
            BaseAddress = new Uri("https://api.kavenegar.com/")
        };
        var options = new SmsOptions();
        var deliveryStore = new FakeSmsDeliveryStore();
        var dateTimeProvider = new FixedDateTimeProvider(new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc));
        var sender = new KavenegarSmsSender(httpClient, options, NullLogger<KavenegarSmsSender>.Instance, deliveryStore, dateTimeProvider);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => sender.SendAsync("09120000000", "test"));
        Assert.Contains("کاوه‌نگار", exception.Message);

        var record = Assert.Single(deliveryStore.Records);
        Assert.Equal(SmsDeliveryStatus.Failed, record.Status);
        Assert.Equal("configuration", record.ErrorCode);
        Assert.Equal("کلید API تنظیم نشده است.", record.ErrorMessage);
    }

    [Fact]
    public async Task SendAsync_Throws_WhenGatewayReturnsFailure()
    {
        const string responseContent = "{\"return\":{\"status\":400}}";
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(responseContent)
        };
        var handler = new FakeHttpMessageHandler(response);
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.kavenegar.com/")
        };
        var options = new SmsOptions
        {
            KavenegarApiKey = "another-api-key"
        };
        var deliveryStore = new FakeSmsDeliveryStore();
        var dateTimeProvider = new FixedDateTimeProvider(new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc));
        var sender = new KavenegarSmsSender(httpClient, options, NullLogger<KavenegarSmsSender>.Instance, deliveryStore, dateTimeProvider);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sender.SendAsync("09120000000", "test"));

        var record = Assert.Single(deliveryStore.Records);
        Assert.Equal(SmsDeliveryStatus.Failed, record.Status);
    Assert.Equal("400", record.ErrorCode);
    Assert.Contains("پاسخ ناموفق", record.ErrorMessage);
    Assert.Equal(responseContent, record.Payload);
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public FakeHttpMessageHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        public HttpRequestMessage? LastRequest { get; private set; }

        public IReadOnlyDictionary<string, string> LastFormValues => _lastFormValues;

        private Dictionary<string, string> _lastFormValues = new();

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;

            if (request.Content is not null)
            {
                var payload = await request.Content.ReadAsStringAsync(cancellationToken);
                _lastFormValues = ParseFormUrlEncoded(payload);
            }

            return _response;
        }

        private static Dictionary<string, string> ParseFormUrlEncoded(string payload)
        {
            var dictionary = new Dictionary<string, string>(StringComparer.Ordinal);

            if (string.IsNullOrEmpty(payload))
            {
                return dictionary;
            }

            var pairs = payload.Split('&', StringSplitOptions.RemoveEmptyEntries);
            foreach (var pair in pairs)
            {
                var keyValue = pair.Split('=', 2);
                var key = Uri.UnescapeDataString(keyValue[0]);
                var valueSegment = keyValue.Length > 1 ? keyValue[1].Replace('+', ' ') : string.Empty;
                var value = Uri.UnescapeDataString(valueSegment);
                dictionary[key] = value;
            }

            return dictionary;
        }
    }

    private sealed class FakeSmsDeliveryStore : ISmsDeliveryStore
    {
        private readonly List<SmsDeliveryRecord> _records = new();

        public IReadOnlyList<SmsDeliveryRecord> Records => _records;

        public Task RecordAsync(SmsDeliveryRecord record, CancellationToken cancellationToken = default)
        {
            _records.Add(record);
            return Task.CompletedTask;
        }
    }

    private sealed class FixedDateTimeProvider : IDateTimeProvider
    {
        public FixedDateTimeProvider(DateTime utcNow)
        {
            UtcNow = utcNow;
        }

        public DateTime UtcNow { get; }
    }
}
