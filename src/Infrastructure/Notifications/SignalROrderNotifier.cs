using System;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Common.Interfaces.Orders;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace EazyMenu.Infrastructure.Notifications;

/// <summary>
/// SignalR implementation of order real-time notifier.
/// </summary>
public sealed class SignalROrderNotifier : IOrderRealtimeNotifier
{
    private const string TenantGroupPrefix = "Tenant_";
    private readonly IHubContext<OrderAlertsHub> _hubContext;
    private readonly ILogger<SignalROrderNotifier> _logger;

    public SignalROrderNotifier(
        IHubContext<OrderAlertsHub> hubContext,
        ILogger<SignalROrderNotifier> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task PublishOrderChangedAsync(OrderRealtimeNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            var groupName = $"{TenantGroupPrefix}{notification.TenantId}";
            
            // Map change type to SignalR event name
            var eventName = notification.ChangeType.ToLowerInvariant() switch
            {
                "order-created" => "orderCreated",
                "order-confirmed" => "orderConfirmed",
                "order-completed" => "orderCompleted",
                "order-cancelled" => "orderCancelled",
                _ => "orderChanged"
            };

            var payload = new
            {
                tenantId = notification.TenantId.ToString(),
                orderId = notification.OrderId.ToString(),
                orderNumber = notification.OrderNumber,
                changeType = notification.ChangeType,
                status = notification.Status,
                totalAmount = notification.TotalAmount,
                timestamp = notification.OccurredAtUtc ?? DateTime.UtcNow
            };

            await _hubContext.Clients.Group(groupName).SendAsync(eventName, payload, cancellationToken);
            
            _logger.LogInformation(
                "اعلان تغییر سفارش {OrderNumber} (نوع: {ChangeType}) به تنانت {TenantId} ارسال شد",
                notification.OrderNumber,
                notification.ChangeType,
                notification.TenantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "خطا در ارسال اعلان تغییر سفارش {OrderNumber} (نوع: {ChangeType})",
                notification.OrderNumber,
                notification.ChangeType);
        }
    }
}
