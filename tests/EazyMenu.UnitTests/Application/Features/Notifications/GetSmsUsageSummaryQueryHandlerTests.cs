using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Common.Interfaces;
using EazyMenu.Application.Common.Interfaces.Notifications;
using EazyMenu.Application.Common.Notifications;
using EazyMenu.Application.Features.Notifications.GetSmsUsageSummary;
using EazyMenu.Domain.Aggregates.Tenants;
using Xunit;

namespace EazyMenu.UnitTests.Application.Features.Notifications;

public sealed class GetSmsUsageSummaryQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenNoPeriodProvided_UsesCurrentMonthAndReturnsAllPlans()
    {
        var period = new DateOnly(2025, 9, 1);
        var usageReader = new FakeUsageReader(period, new List<SmsUsageAggregate>
        {
            new(null, 5, 2, 7),
            new(SubscriptionPlan.Starter, 120, 3, 123),
            new(SubscriptionPlan.Pro, 1400, 12, 1412)
        });
        var handler = new GetSmsUsageSummaryQueryHandler(usageReader, new FixedDateTimeProvider(new DateTime(2025, 9, 15, 8, 0, 0, DateTimeKind.Utc)));

        var result = await handler.HandleAsync(new GetSmsUsageSummaryQuery(), CancellationToken.None);

        Assert.Equal(period, result.Period);
        Assert.Equal(5, result.Items.Count); // 4 plans + unknown

        var starter = Assert.Single(result.Items, item => item.PlanLabel.Contains("استارتر"));
        Assert.Equal(500, starter.IncludedMessages);
        Assert.Equal(120, starter.SentMessages);
        Assert.Equal(3, starter.FailedMessages);
        Assert.Equal(123, starter.TotalMessages);
        Assert.Equal(380, starter.RemainingMessages);
        Assert.Equal(24m, starter.UsagePercentage);

        var unknown = Assert.Single(result.Items, item => item.PlanLabel == "نامشخص");
        Assert.Equal(0, unknown.IncludedMessages);
        Assert.Equal(5, unknown.SentMessages);
        Assert.Equal(2, unknown.FailedMessages);
        Assert.Equal(7, unknown.TotalMessages);
    }

    private sealed class FakeUsageReader : ISmsUsageReader
    {
        private readonly DateOnly _expectedPeriod;
        private readonly IReadOnlyCollection<SmsUsageAggregate> _aggregates;

        public FakeUsageReader(DateOnly expectedPeriod, IReadOnlyCollection<SmsUsageAggregate> aggregates)
        {
            _expectedPeriod = expectedPeriod;
            _aggregates = aggregates;
        }

        public Task<IReadOnlyCollection<SmsUsageAggregate>> GetMonthlyUsageAsync(DateOnly month, CancellationToken cancellationToken = default)
        {
            Assert.Equal(_expectedPeriod, month);
            return Task.FromResult(_aggregates);
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
