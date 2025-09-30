using System;
using System.Collections.Generic;

namespace EazyMenu.Application.Common.Notifications;

/// <summary>
/// نتیجه کوئری خلاصه مصرف پیامک ماهانه به تفکیک پلن.
/// </summary>
public sealed record SmsUsageSummary(DateOnly Period, IReadOnlyList<SmsUsageSummaryItem> Items);

/// <summary>
/// آیتم مصرف پیامک برای یک پلن مشخص یا مقدار ناشناس.
/// </summary>
public sealed record SmsUsageSummaryItem(
    string PlanLabel,
    int IncludedMessages,
    int SentMessages,
    int FailedMessages,
    int TotalMessages,
    int RemainingMessages,
    decimal UsagePercentage);
