using System.Threading;
using System.Threading.Tasks;

namespace EazyMenu.Application.Common.Interfaces.Notifications;

/// <summary>
/// ارسال اعلان ایمیلی برای مدیران/پشتیبانی سیستم.
/// </summary>
public interface IEmailSender
{
    Task SendAsync(string recipient, string subject, string body, CancellationToken cancellationToken = default);
}
