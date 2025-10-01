using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Domain.Aggregates.Orders;
using EazyMenu.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EazyMenu.Infrastructure.Persistence.Repositories;

internal sealed class OrderRepository : IOrderRepository
{
    private readonly EazyMenuDbContext _dbContext;

    public OrderRepository(EazyMenuDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Order?> GetByIdAsync(TenantId tenantId, OrderId orderId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .Where(o => o.TenantId == tenantId && o.Id == orderId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Order>> GetRecentAsync(TenantId tenantId, int take, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .Where(o => o.TenantId == tenantId)
            .OrderByDescending(o => o.CreatedAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Order>> GetRecentOrdersAsync(TenantId tenantId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var skip = (pageNumber - 1) * pageSize;
        
        return await _dbContext.Orders
            .Where(o => o.TenantId == tenantId)
            .OrderByDescending(o => o.CreatedAtUtc)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        await _dbContext.Orders.AddAsync(order, cancellationToken);
    }

    public Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        _dbContext.Orders.Update(order);
        return Task.CompletedTask;
    }
}
