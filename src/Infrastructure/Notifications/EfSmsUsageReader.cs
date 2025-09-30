using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Common.Interfaces.Notifications;
using EazyMenu.Application.Common.Notifications;
using EazyMenu.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EazyMenu.Infrastructure.Notifications;

internal sealed class EfSmsUsageReader : ISmsUsageReader
{
    private readonly EazyMenuDbContext _dbContext;

    public EfSmsUsageReader(EazyMenuDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<SmsUsageAggregate>> GetMonthlyUsageAsync(DateOnly month, CancellationToken cancellationToken = default)
    {
        var start = new DateTimeOffset(month.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var end = start.AddMonths(1);

        var aggregates = await _dbContext.SmsDeliveryLogs
            .AsNoTracking()
            .Where(log => log.OccurredAt >= start && log.OccurredAt < end)
            .GroupBy(log => log.SubscriptionPlan)
            .Select(group => new SmsUsageAggregate(
                group.Key,
                group.Count(log => log.Status == SmsDeliveryStatus.Sent),
                group.Count(log => log.Status == SmsDeliveryStatus.Failed),
                group.Count()))
            .ToListAsync(cancellationToken);

        return aggregates;
    }
}
