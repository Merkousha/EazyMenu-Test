using System;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Common.Interfaces.Menus;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace EazyMenu.Infrastructure.Menus;

internal sealed class SignalRMenuRealtimeNotifier : IMenuRealtimeNotifier
{
    private readonly IHubContext<MenuUpdatesHub> _hubContext;
    private readonly ILogger<SignalRMenuRealtimeNotifier> _logger;

    public SignalRMenuRealtimeNotifier(IHubContext<MenuUpdatesHub> hubContext, ILogger<SignalRMenuRealtimeNotifier> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task PublishMenuChangedAsync(MenuRealtimeNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                tenantId = notification.TenantId,
                menuId = notification.MenuId,
                changeType = notification.ChangeType,
                categoryId = notification.CategoryId,
                itemId = notification.ItemId,
                publishedVersion = notification.PublishedVersion,
                occurredAt = DateTime.UtcNow
            };

            await _hubContext.Clients
                .Group(MenuUpdatesHub.GetTenantGroup(notification.TenantId))
                .SendAsync("menuChanged", payload, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "خطا در ارسال پیام همگام‌سازی منو از طریق SignalR");
        }
    }
}
