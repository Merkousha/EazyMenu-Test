using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Domain.Aggregates.Orders;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Abstractions.Persistence;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(TenantId tenantId, OrderId orderId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Order>> GetRecentAsync(TenantId tenantId, int take, CancellationToken cancellationToken = default);

    Task AddAsync(Order order, CancellationToken cancellationToken = default);

    Task UpdateAsync(Order order, CancellationToken cancellationToken = default);
}
