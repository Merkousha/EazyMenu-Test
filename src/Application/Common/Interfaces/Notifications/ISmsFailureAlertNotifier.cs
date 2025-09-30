using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Common.Notifications;

namespace EazyMenu.Application.Common.Interfaces.Notifications;

/// <summary>
/// اعلان فوری شکست‌های پیامک برای مصرف‌کنندگان رابط کاربری.
/// </summary>
public interface ISmsFailureAlertNotifier
{
    Task PublishAsync(SmsFailureAlert alert, CancellationToken cancellationToken = default);
}
