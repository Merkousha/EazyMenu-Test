using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Common.Interfaces.Orders;
using EazyMenu.Domain.Aggregates.Orders;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Orders.PlaceOrder;

public sealed class PlaceOrderCommandHandler : ICommandHandler<PlaceOrderCommand, OrderId>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderNumberGenerator _orderNumberGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public PlaceOrderCommandHandler(
        IOrderRepository orderRepository,
        IOrderNumberGenerator orderNumberGenerator,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _orderNumberGenerator = orderNumberGenerator;
        _unitOfWork = unitOfWork;
    }

    public async Task<OrderId> HandleAsync(PlaceOrderCommand command, CancellationToken cancellationToken = default)
    {
        if (!TenantId.TryCreate(command.TenantId, out var tenantId))
        {
            throw new BusinessRuleViolationException("شناسه مستاجر معتبر نیست.");
        }

        PhoneNumber customerPhone;
        try
        {
            customerPhone = PhoneNumber.Create(command.CustomerPhone);
        }
        catch
        {
            throw new BusinessRuleViolationException("شماره تلفن معتبر نیست.");
        }

        if (!Enum.TryParse<FulfillmentMethod>(command.FulfillmentMethod, out var fulfillmentMethod))
        {
            throw new BusinessRuleViolationException("روش تحویل معتبر نیست.");
        }

        if (command.Items == null || !command.Items.Any())
        {
            throw new BusinessRuleViolationException("سفارش باید حداقل یک آیتم داشته باشد.");
        }

        var orderNumber = await _orderNumberGenerator.GenerateAsync(tenantId, DateTime.UtcNow, cancellationToken);

        var itemDrafts = command.Items.Select(item =>
            new Order.OrderItemDraft(
                item.MenuItemId,
                item.DisplayName,
                item.UnitPrice,
                item.Quantity,
                item.Note)).ToList();

        var order = Order.Create(
            tenantId,
            command.MenuId,
            orderNumber,
            fulfillmentMethod,
            command.CustomerName,
            customerPhone,
            command.CustomerNote,
            command.DeliveryFee,
            command.TaxAmount,
            itemDrafts);

        await _orderRepository.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return order.Id;
    }
}
