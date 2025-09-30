using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Common.Interfaces;
using EazyMenu.Application.Common.Interfaces.Notifications;
using EazyMenu.Application.Common.Notifications;
using EazyMenu.Domain.Aggregates.Tenants;

namespace EazyMenu.Application.Features.Notifications.GetSmsUsageSummary;

public sealed class GetSmsUsageSummaryQueryHandler : IQueryHandler<GetSmsUsageSummaryQuery, SmsUsageSummary>
{
    private readonly ISmsUsageReader _usageReader;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetSmsUsageSummaryQueryHandler(ISmsUsageReader usageReader, IDateTimeProvider dateTimeProvider)
    {
        _usageReader = usageReader;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<SmsUsageSummary> HandleAsync(GetSmsUsageSummaryQuery query, CancellationToken cancellationToken = default)
    {
        var period = ResolvePeriod(query);
        var aggregates = await _usageReader.GetMonthlyUsageAsync(period, cancellationToken);
        var aggregateLookup = new Dictionary<SubscriptionPlan, SmsUsageAggregate>();
        SmsUsageAggregate? unknownAggregate = null;

        foreach (var aggregate in aggregates)
        {
            if (aggregate.SubscriptionPlan is { } plan)
            {
                aggregateLookup[plan] = aggregate;
            }
            else
            {
                unknownAggregate = aggregate;
            }
        }

        var items = new List<SmsUsageSummaryItem>();

        foreach (var plan in Enum.GetValues<SubscriptionPlan>())
        {
            aggregateLookup.TryGetValue(plan, out var aggregate);
            var sent = aggregate?.SentCount ?? 0;
            var failed = aggregate?.FailedCount ?? 0;
            var total = aggregate?.TotalCount ?? (sent + failed);
            var included = SmsPlanQuota.GetMonthlyIncludedMessages(plan);
            var remaining = included > sent ? included - sent : 0;
            var usagePercentage = included == 0
                ? 0m
                : Math.Min(100m, Math.Round((decimal)sent / included * 100m, 2, MidpointRounding.AwayFromZero));

            items.Add(new SmsUsageSummaryItem(
                GetPlanLabel(plan),
                included,
                sent,
                failed,
                total,
                remaining,
                usagePercentage));
        }

        if (unknownAggregate is not null)
        {
            var sent = unknownAggregate.SentCount;
            var failed = unknownAggregate.FailedCount;
            var total = unknownAggregate.TotalCount;
            items.Add(new SmsUsageSummaryItem(
                "نامشخص",
                0,
                sent,
                failed,
                total,
                0,
                0));
        }

        return new SmsUsageSummary(period, items);
    }

    private DateOnly ResolvePeriod(GetSmsUsageSummaryQuery query)
    {
        if (query.Year.HasValue && query.Month.HasValue)
        {
            return new DateOnly(query.Year.Value, query.Month.Value, 1);
        }

        var utcNow = _dateTimeProvider.UtcNow;
        return new DateOnly(utcNow.Year, utcNow.Month, 1);
    }

    private static string GetPlanLabel(SubscriptionPlan plan) => plan switch
    {
        SubscriptionPlan.Trial => "پلن آزمایشی",
        SubscriptionPlan.Starter => "پلن استارتر",
        SubscriptionPlan.Pro => "پلن پرو",
        SubscriptionPlan.Enterprise => "پلن اینترپرایز",
        _ => plan.ToString()
    };
}
