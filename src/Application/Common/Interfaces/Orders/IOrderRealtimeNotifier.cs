using System;
using System.Threading;
using System.Threading.Tasks;

namespace EazyMenu.Application.Common.Interfaces.Orders;

public interface IOrderRealtimeNotifier
{
    Task PublishOrderChangedAsync(OrderRealtimeNotification notification, CancellationToken cancellationToken = default);
}

public sealed record OrderRealtimeNotification(
    Guid TenantId,
    Guid OrderId,
    string OrderNumber,
    string ChangeType,
    string? Status = null,
    decimal? TotalAmount = null,
    DateTime? OccurredAtUtc = null);
