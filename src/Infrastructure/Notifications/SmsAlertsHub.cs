using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace EazyMenu.Infrastructure.Notifications;

/// <summary>
/// هاب SignalR برای پخش اعلان‌های شکست پیامک به داشبورد مدیریتی.
/// </summary>
public sealed class SmsAlertsHub : Hub
{
    public Task JoinDashboard() => Groups.AddToGroupAsync(Context.ConnectionId, "dashboard");
}
