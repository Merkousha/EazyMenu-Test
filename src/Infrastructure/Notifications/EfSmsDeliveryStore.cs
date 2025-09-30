using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Common.Interfaces.Notifications;
using EazyMenu.Application.Common.Notifications;
using EazyMenu.Infrastructure.Persistence;
using EazyMenu.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace EazyMenu.Infrastructure.Notifications;

internal sealed class EfSmsDeliveryStore : ISmsDeliveryStore, ISmsDeliveryLogReader
{
    private readonly EazyMenuDbContext _dbContext;

    public EfSmsDeliveryStore(EazyMenuDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task RecordAsync(SmsDeliveryRecord record, CancellationToken cancellationToken = default)
    {
        if (record is null)
        {
            throw new ArgumentNullException(nameof(record));
        }

        var entity = new SmsDeliveryLog
        {
            Id = record.Id,
            PhoneNumber = record.PhoneNumber,
            Message = record.Message,
            Provider = record.Provider,
            Status = record.Status,
            OccurredAt = record.OccurredAt,
            ErrorCode = record.ErrorCode,
            ErrorMessage = record.ErrorMessage,
            Payload = record.Payload,
            TenantId = record.TenantId,
            SubscriptionPlan = record.SubscriptionPlan
        };

        await _dbContext.SmsDeliveryLogs.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<SmsDeliveryLogPage> GetAsync(
        SmsDeliveryStatus? status,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        if (skip < 0)
        {
            skip = 0;
        }

        if (take is < 1 or > 200)
        {
            take = 50;
        }

        var query = _dbContext.SmsDeliveryLogs
            .AsNoTracking()
            .OrderByDescending(log => log.OccurredAt)
            .ThenByDescending(log => log.Id)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(log => log.Status == status.Value);
        }

        var logs = await query
            .Skip(skip)
            .Take(take + 1)
            .ToListAsync(cancellationToken);

        var hasMore = logs.Count > take;
        if (hasMore)
        {
            logs.RemoveAt(logs.Count - 1);
        }

        IReadOnlyCollection<SmsDeliveryRecord> records = logs
            .Select(log => new SmsDeliveryRecord(
                log.Id,
                log.PhoneNumber,
                log.Message,
                log.Provider,
                log.Status,
                log.OccurredAt,
                log.ErrorCode,
                log.ErrorMessage,
                log.Payload,
                log.TenantId,
                log.SubscriptionPlan))
            .ToArray();

        return new SmsDeliveryLogPage(records, hasMore);
    }
}
