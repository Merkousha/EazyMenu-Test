using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Common.Interfaces;
using EazyMenu.Application.Common.Interfaces.Notifications;
using EazyMenu.Application.Common.Notifications;
using EazyMenu.Infrastructure.Notifications;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace EazyMenu.UnitTests.Infrastructure.Notifications;

public class LoggingSmsSenderTests
{
    [Fact]
    public async Task SendAsync_RecordsDeliveryLog()
    {
        var store = new FakeSmsDeliveryStore();
        var dateTimeProvider = new FixedDateTimeProvider(new DateTime(2025, 1, 1, 10, 30, 0, DateTimeKind.Utc));
        var sender = new LoggingSmsSender(
            NullLogger<LoggingSmsSender>.Instance,
            store,
            dateTimeProvider);

        await sender.SendAsync("09120000000", "کد ورود شما 1234", CancellationToken.None);

        var record = Assert.Single(store.Records);
        Assert.Equal(SmsDeliveryStatus.Sent, record.Status);
        Assert.Equal("09120000000", record.PhoneNumber);
        Assert.Equal("کد ورود شما 1234", record.Message);
        Assert.Equal(nameof(SmsProvider.Logging), record.Provider);
        Assert.Equal(dateTimeProvider.UtcNow, record.OccurredAt.UtcDateTime);
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
