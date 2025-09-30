using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Common.Notifications;
using EazyMenu.Application.Features.Notifications.GetSmsDeliveryLogs;
using EazyMenu.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace EazyMenu.Web.Controllers;

public sealed class NotificationsController : Controller
{
    private const int DefaultPageSize = 20;

    private readonly ILogger<NotificationsController> _logger;
    private readonly IQueryHandler<GetSmsDeliveryLogsQuery, SmsDeliveryLogPage> _getSmsDeliveryLogsQueryHandler;

    public NotificationsController(
        ILogger<NotificationsController> logger,
        IQueryHandler<GetSmsDeliveryLogsQuery, SmsDeliveryLogPage> getSmsDeliveryLogsQueryHandler)
    {
        _logger = logger;
        _getSmsDeliveryLogsQueryHandler = getSmsDeliveryLogsQueryHandler;
    }

    [HttpGet]
    public async Task<IActionResult> SmsLogs(int page = 1, SmsDeliveryStatus? status = null, CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            page = 1;
        }

        try
        {
            var response = await _getSmsDeliveryLogsQueryHandler.HandleAsync(
                new GetSmsDeliveryLogsQuery(page, DefaultPageSize, status),
                cancellationToken);

            var items = response.Items
                .Select(record => new SmsDeliveryLogEntryViewModel(
                    record.Id,
                    record.PhoneNumber,
                    record.Provider,
                    record.Status,
                    record.OccurredAt,
                    record.Message,
                    record.ErrorCode,
                    record.ErrorMessage,
                    record.Payload))
                .ToList();

            var viewModel = new SmsDeliveryLogsViewModel(
                items,
                BuildStatusOptions(status),
                page,
                DefaultPageSize,
                response.HasMore,
                status);

            ViewData["Title"] = "گزارش پیامک‌ها";
            return View(viewModel);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "خطا در دریافت گزارش پیامک‌ها");
            ModelState.AddModelError(string.Empty, "در بازیابی گزارش پیامک‌ها خطایی رخ داد. لطفاً دوباره تلاش کنید.");
            var fallbackModel = new SmsDeliveryLogsViewModel(
                Array.Empty<SmsDeliveryLogEntryViewModel>(),
                BuildStatusOptions(status),
                page,
                DefaultPageSize,
                hasMore: false,
                status);
            return View(fallbackModel);
        }
    }

    private static IReadOnlyList<SelectListItem> BuildStatusOptions(SmsDeliveryStatus? selectedStatus)
    {
        var options = new List<SelectListItem>
        {
            new("همه وضعیت‌ها", string.Empty)
            {
                Selected = selectedStatus is null
            }
        };

        foreach (SmsDeliveryStatus status in Enum.GetValues(typeof(SmsDeliveryStatus)))
        {
            options.Add(new SelectListItem(GetStatusLabel(status), status.ToString())
            {
                Selected = selectedStatus == status
            });
        }

        return options;
    }

    private static string GetStatusLabel(SmsDeliveryStatus status) => status switch
    {
        SmsDeliveryStatus.Pending => "در صف ارسال",
        SmsDeliveryStatus.Sent => "ارسال موفق",
        SmsDeliveryStatus.Failed => "ناموفق",
        _ => status.ToString()
    };
}
