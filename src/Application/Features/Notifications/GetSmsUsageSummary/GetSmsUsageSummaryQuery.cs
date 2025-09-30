using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Common.Notifications;

namespace EazyMenu.Application.Features.Notifications.GetSmsUsageSummary;

/// <summary>
/// کوئری درخواست خلاصه مصرف پیامک ماهانه.
/// </summary>
public sealed record GetSmsUsageSummaryQuery(int? Year = null, int? Month = null) : IQuery<SmsUsageSummary>;
