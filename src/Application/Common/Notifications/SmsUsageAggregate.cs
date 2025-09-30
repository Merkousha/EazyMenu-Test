using EazyMenu.Domain.Aggregates.Tenants;

namespace EazyMenu.Application.Common.Notifications;

/// <summary>
/// خلاصه آماری ارسال پیامک برای یک پلن اشتراک در بازه مشخص.
/// </summary>
public sealed record SmsUsageAggregate(SubscriptionPlan? SubscriptionPlan, int SentCount, int FailedCount, int TotalCount);
