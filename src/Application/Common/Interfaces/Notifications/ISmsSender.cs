using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Common.Notifications;

namespace EazyMenu.Application.Common.Interfaces.Notifications;

/// <summary>
/// سرویس ارسال پیامک برای ارسال کدهای ورود و اعلان‌های تراکنشی.
/// </summary>
public interface ISmsSender
{
    Task SendAsync(string phoneNumber, string message, SmsSendContext? context = null, CancellationToken cancellationToken = default);
}
