using System;
using System.Linq;
using System.Threading.Tasks;
using EazyMenu.Application.Common.Notifications;
using EazyMenu.Infrastructure.Notifications;
using EazyMenu.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EazyMenu.IntegrationTests.Infrastructure.Notifications;

public class EfSmsDeliveryStoreTests
{
    [Fact]
    public async Task GetAsync_ReturnsPagedResultsOrderedByOccurrence()
    {
        var options = new DbContextOptionsBuilder<EazyMenuDbContext>()
            .UseInMemoryDatabase($"SmsDeliveryLogs-{Guid.NewGuid()}")
            .Options;

        await using var dbContext = new EazyMenuDbContext(options);
        var store = new EfSmsDeliveryStore(dbContext);

        var baseTime = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);

        var older = new SmsDeliveryRecord(
            Guid.NewGuid(),
            "09120000001",
            "پیام قدیمی",
            "logger",
            SmsDeliveryStatus.Sent,
            baseTime.AddMinutes(-30));

        var failed = new SmsDeliveryRecord(
            Guid.NewGuid(),
            "09120000002",
            "پیام ناموفق",
            "logger",
            SmsDeliveryStatus.Failed,
            baseTime.AddMinutes(-10),
            ErrorCode: "network",
            ErrorMessage: "قطع ارتباط",
            Payload: "{}");

        var newest = new SmsDeliveryRecord(
            Guid.NewGuid(),
            "09120000003",
            "پیام جدید",
            "logger",
            SmsDeliveryStatus.Sent,
            baseTime);

        await store.RecordAsync(older);
        await store.RecordAsync(failed);
        await store.RecordAsync(newest);

        var firstPage = await store.GetAsync(null, skip: 0, take: 2);

    Assert.True(firstPage.HasMore);
    Assert.Equal(2, firstPage.Items.Count);
    Assert.Equal(newest.Id, firstPage.Items.First().Id);
    Assert.Equal(failed.Id, firstPage.Items.ElementAt(1).Id);

        var secondPage = await store.GetAsync(null, skip: 2, take: 2);

        Assert.False(secondPage.HasMore);
    var remaining = Assert.Single(secondPage.Items);
        Assert.Equal(older.Id, remaining.Id);

        var failedOnly = await store.GetAsync(SmsDeliveryStatus.Failed, skip: 0, take: 5);

        Assert.False(failedOnly.HasMore);
    var failedRecord = Assert.Single(failedOnly.Items);
        Assert.Equal(failed.Id, failedRecord.Id);
        Assert.Equal("network", failedRecord.ErrorCode);
    }
}
