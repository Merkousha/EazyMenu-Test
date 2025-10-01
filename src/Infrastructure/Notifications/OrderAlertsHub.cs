using Microsoft.AspNetCore.SignalR;

namespace EazyMenu.Infrastructure.Notifications;

/// <summary>
/// SignalR hub for broadcasting order events to connected dashboard clients.
/// </summary>
public sealed class OrderAlertsHub : Hub
{
    private const string TenantGroupPrefix = "Tenant_";

    /// <summary>
    /// Subscribes the client to order notifications for a specific tenant.
    /// </summary>
    public async Task SubscribeToTenant(string tenantId)
    {
        if (!string.IsNullOrWhiteSpace(tenantId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"{TenantGroupPrefix}{tenantId}");
        }
    }

    /// <summary>
    /// Unsubscribes the client from order notifications for a specific tenant.
    /// </summary>
    public async Task UnsubscribeFromTenant(string tenantId)
    {
        if (!string.IsNullOrWhiteSpace(tenantId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"{TenantGroupPrefix}{tenantId}");
        }
    }
}
