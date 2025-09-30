using System;
using System.Collections.Generic;
using EazyMenu.Application.Common.Notifications;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EazyMenu.Web.Models;

/// <summary>
/// مدل نمایش گزارش پیامک‌ها به همراه اطلاعات صفحه‌بندی و فیلتر وضعیت.
/// </summary>
public sealed class SmsDeliveryLogsViewModel
{
    public SmsDeliveryLogsViewModel(
        IReadOnlyList<SmsDeliveryLogEntryViewModel> items,
        IReadOnlyList<SelectListItem> statusOptions,
        int page,
        int pageSize,
        bool hasMore,
        SmsDeliveryStatus? status,
        SmsUsageSummary? usageSummary)
    {
        Items = items;
        StatusOptions = statusOptions;
        Page = page;
        PageSize = pageSize;
        HasMore = hasMore;
        Status = status;
        UsageSummary = usageSummary;
    }

    public IReadOnlyList<SmsDeliveryLogEntryViewModel> Items { get; }

    public IReadOnlyList<SelectListItem> StatusOptions { get; }

    public int Page { get; }

    public int PageSize { get; }

    public bool HasMore { get; }

    public SmsDeliveryStatus? Status { get; }

    public SmsUsageSummary? UsageSummary { get; }
}

/// <summary>
/// رکورد نمایش یک لاگ ارسال پیامک در داشبورد.
/// </summary>
public sealed record SmsDeliveryLogEntryViewModel(
    Guid Id,
    string PhoneNumber,
    string Provider,
    SmsDeliveryStatus Status,
    DateTimeOffset OccurredAt,
    string Message,
    string? ErrorCode,
    string? ErrorMessage,
    string? Payload);
