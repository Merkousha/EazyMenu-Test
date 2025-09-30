using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Common.Notifications;

namespace EazyMenu.Application.Common.Interfaces.Notifications;

/// <summary>
/// سرویسی برای واکشی لاگ‌های ارسال پیامک جهت نمایش در داشبورد مدیریتی.
/// </summary>
public interface ISmsDeliveryLogReader
{
    Task<SmsDeliveryLogPage> GetAsync(
        SmsDeliveryStatus? status,
        int skip,
        int take,
        CancellationToken cancellationToken = default);
}
