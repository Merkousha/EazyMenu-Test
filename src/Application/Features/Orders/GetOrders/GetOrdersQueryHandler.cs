using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Features.Orders.Common;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Orders.GetOrders;

public sealed class GetOrdersQueryHandler : IQueryHandler<GetOrdersQuery, IReadOnlyCollection<OrderSummaryDto>>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrdersQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<IReadOnlyCollection<OrderSummaryDto>> HandleAsync(GetOrdersQuery query, CancellationToken cancellationToken = default)
    {
        if (!TenantId.TryCreate(query.TenantId, out var tenantId))
        {
            throw new BusinessRuleViolationException("شناسه مستاجر معتبر نیست.");
        }

        var orders = await _orderRepository.GetRecentOrdersAsync(tenantId, query.PageNumber, query.PageSize, cancellationToken);

        // Filter by status if specified
        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            orders = orders.Where(o => o.Status.ToString() == query.Status).ToList();
        }

        return orders
            .Select(OrderMapper.ToSummaryDto)
            .ToList();
    }
}
