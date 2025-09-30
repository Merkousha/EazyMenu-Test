using System.Collections.Generic;
using EazyMenu.Domain.Aggregates.Tenants;

namespace EazyMenu.Application.Common.Notifications;

/// <summary>
/// سهمیه ماهانه پیامک بر اساس پلن اشتراک.
/// </summary>
public static class SmsPlanQuota
{
    private static readonly IReadOnlyDictionary<SubscriptionPlan, int> Quotas = new Dictionary<SubscriptionPlan, int>
    {
        [SubscriptionPlan.Trial] = 100,
        [SubscriptionPlan.Starter] = 500,
        [SubscriptionPlan.Pro] = 2000,
        [SubscriptionPlan.Enterprise] = 10000
    };

    public static int GetMonthlyIncludedMessages(SubscriptionPlan plan)
    {
        return Quotas.TryGetValue(plan, out var quota) ? quota : 0;
    }
}
