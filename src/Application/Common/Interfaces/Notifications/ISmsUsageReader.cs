using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Common.Notifications;

namespace EazyMenu.Application.Common.Interfaces.Notifications;

/// <summary>
/// سرویس خوانش آمار مصرف پیامک جهت تحلیل مصرف ماهانه بر اساس پلن اشتراک.
/// </summary>
public interface ISmsUsageReader
{
    Task<IReadOnlyCollection<SmsUsageAggregate>> GetMonthlyUsageAsync(DateOnly month, CancellationToken cancellationToken = default);
}
