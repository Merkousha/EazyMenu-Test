using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace EazyMenu.Infrastructure.Menus;

/// <summary>
/// هاب SignalR برای همگام‌سازی آنی تغییرات منو میان داشبورد و سایت عمومی.
/// </summary>
public sealed class MenuUpdatesHub : Hub
{
    public Task JoinTenant(Guid tenantId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, GetTenantGroup(tenantId));
    }

    internal static string GetTenantGroup(Guid tenantId) => $"tenant:{tenantId:D}";
}
