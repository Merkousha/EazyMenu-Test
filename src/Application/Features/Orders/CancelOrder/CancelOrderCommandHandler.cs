using System;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Common.Interfaces.Orders;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Orders.CancelOrder;

public sealed class CancelOrderCommandHandler : ICommandHandler<CancelOrderCommand, bool>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderRealtimeNotifier _realtimeNotifier;

    public CancelOrderCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        IOrderRealtimeNotifier realtimeNotifier)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _realtimeNotifier = realtimeNotifier;
    }

    public async Task<bool> HandleAsync(CancelOrderCommand command, CancellationToken cancellationToken = default)
    {
        if (!TenantId.TryCreate(command.TenantId, out var tenantId))
        {
            throw new BusinessRuleViolationException("شناسه مستاجر معتبر نیست.");
        }

        if (!OrderId.TryCreate(command.OrderId, out var orderId))
        {
            throw new BusinessRuleViolationException("شناسه سفارش معتبر نیست.");
        }

        var order = await _orderRepository.GetByIdAsync(tenantId, orderId, cancellationToken);
        if (order is null)
        {
            throw new NotFoundException("سفارش مورد نظر یافت نشد.");
        }

        order.Cancel(command.Reason);

        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Broadcast real-time notification
        await _realtimeNotifier.PublishOrderChangedAsync(
            new OrderRealtimeNotification(
                TenantId: order.TenantId.Value,
                OrderId: order.Id.Value,
                OrderNumber: order.OrderNumber,
                ChangeType: "order-cancelled",
                Status: order.Status.ToString(),
                TotalAmount: order.TotalAmount,
                OccurredAtUtc: DateTime.UtcNow),
            cancellationToken);

        return true;
    }
}
