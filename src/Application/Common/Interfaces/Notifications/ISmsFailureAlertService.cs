using System;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Common.Notifications;

namespace EazyMenu.Application.Common.Interfaces.Notifications;

/// <summary>
/// سرویس ثبت و اعلام سناریوهای شکست پیامک به کانال‌های جایگزین.
/// </summary>
public interface ISmsFailureAlertService
{
    Task NotifyFailureAsync(string phoneNumber, string message, Exception exception, SmsSendContext? context = null, CancellationToken cancellationToken = default);
}
