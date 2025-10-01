using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Orders.ConfirmOrder;

public sealed class ConfirmOrderCommandHandler : ICommandHandler<ConfirmOrderCommand, bool>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ConfirmOrderCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> HandleAsync(ConfirmOrderCommand command, CancellationToken cancellationToken = default)
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

        order.Confirm();

        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
