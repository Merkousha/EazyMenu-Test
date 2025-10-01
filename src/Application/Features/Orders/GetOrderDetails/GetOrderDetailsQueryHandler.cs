using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Features.Orders.Common;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Orders.GetOrderDetails;

public sealed class GetOrderDetailsQueryHandler : IQueryHandler<GetOrderDetailsQuery, OrderDetailsDto>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderDetailsQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDetailsDto> HandleAsync(GetOrderDetailsQuery query, CancellationToken cancellationToken = default)
    {
        if (!TenantId.TryCreate(query.TenantId, out var tenantId))
        {
            throw new BusinessRuleViolationException("شناسه مستاجر معتبر نیست.");
        }

        if (!OrderId.TryCreate(query.OrderId, out var orderId))
        {
            throw new BusinessRuleViolationException("شناسه سفارش معتبر نیست.");
        }

        var order = await _orderRepository.GetByIdAsync(tenantId, orderId, cancellationToken);
        if (order is null)
        {
            throw new NotFoundException("سفارش مورد نظر یافت نشد.");
        }

        return OrderMapper.ToDetailsDto(order);
    }
}
