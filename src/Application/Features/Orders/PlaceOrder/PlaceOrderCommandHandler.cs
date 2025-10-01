using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Common.Interfaces.Notifications;
using EazyMenu.Application.Common.Interfaces.Orders;
using EazyMenu.Application.Common.Notifications;
using EazyMenu.Domain.Aggregates.Orders;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Orders.PlaceOrder;

public sealed class PlaceOrderCommandHandler : ICommandHandler<PlaceOrderCommand, OrderId>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderNumberGenerator _orderNumberGenerator;
    private readonly IOrderRealtimeNotifier _realtimeNotifier;
    private readonly ISmsSender _smsSender;
    private readonly IUnitOfWork _unitOfWork;

    public PlaceOrderCommandHandler(
        IOrderRepository orderRepository,
        IOrderNumberGenerator orderNumberGenerator,
        IOrderRealtimeNotifier realtimeNotifier,
        ISmsSender smsSender,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _orderNumberGenerator = orderNumberGenerator;
        _realtimeNotifier = realtimeNotifier;
        _smsSender = smsSender;
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

        // Broadcast new order notification
        await _realtimeNotifier.PublishOrderChangedAsync(
            new OrderRealtimeNotification(
                TenantId: command.TenantId,
                OrderId: order.Id.Value,
                OrderNumber: orderNumber,
                ChangeType: "order-created",
                Status: "Pending",
                TotalAmount: order.TotalAmount,
                OccurredAtUtc: DateTime.UtcNow),
            cancellationToken);

        // Send SMS notification to customer
        await SendOrderConfirmationSmsAsync(order, cancellationToken);

        return order.Id;
    }

    /// <summary>
    /// Sends SMS confirmation to customer after order placement.
    /// This method does not throw exceptions to prevent SMS failures from failing order placement.
    /// </summary>
    private async Task SendOrderConfirmationSmsAsync(Order order, CancellationToken cancellationToken)
    {
        try
        {
            var message = $"سفارش شما با شماره {order.OrderNumber} ثبت شد.\n" +
                         $"مبلغ کل: {order.TotalAmount.ToString("N0", new CultureInfo("fa-IR"))} تومان\n" +
                         $"از خرید شما متشکریم.";

            // TODO: Retrieve actual subscription plan from tenant context
            var smsContext = new SmsSendContext(
                TenantId: order.TenantId.Value,
                SubscriptionPlan: Domain.Aggregates.Tenants.SubscriptionPlan.Starter);

            await _smsSender.SendAsync(
                order.CustomerPhone.Value,
                message,
                smsContext,
                cancellationToken);
        }
        catch
        {
            // SMS failures should not fail order placement
            // Logging would be done here in production via infrastructure layer
        }
    }
}
