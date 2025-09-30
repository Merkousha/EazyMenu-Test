using System;
using System.Threading;
using System.Threading.Tasks;

namespace EazyMenu.Application.Common.Interfaces.Notifications;

/// <summary>
/// سرویس ثبت و اعلام سناریوهای شکست پیامک به کانال‌های جایگزین.
/// </summary>
public interface ISmsFailureAlertService
{
    Task NotifyFailureAsync(string phoneNumber, string message, Exception exception, CancellationToken cancellationToken = default);
}
