using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Common.Notifications;

namespace EazyMenu.Application.Common.Interfaces.Notifications;

/// <summary>
/// قرارداد ذخیره‌سازی وضعیت ارسال پیامک برای گزارش‌گیری و مانیتورینگ.
/// </summary>
public interface ISmsDeliveryStore
{
    Task RecordAsync(SmsDeliveryRecord record, CancellationToken cancellationToken = default);
}
