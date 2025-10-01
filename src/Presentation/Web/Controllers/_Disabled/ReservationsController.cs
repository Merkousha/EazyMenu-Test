using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Domain.Common.Exceptions;
using EazyMenu.Application.Features.Reservations.CancelReservation;
using EazyMenu.Application.Features.Reservations.CheckInReservation;
using EazyMenu.Application.Features.Reservations.Common;
using EazyMenu.Application.Features.Reservations.ConfirmReservation;
using EazyMenu.Application.Features.Reservations.GetBranchTables;
using EazyMenu.Application.Features.Reservations.GetReservationsForDay;
using EazyMenu.Application.Features.Reservations.MarkReservationNoShow;
using EazyMenu.Application.Features.Reservations.ScheduleReservation;
using EazyMenu.Domain.ValueObjects;
using EazyMenu.Web.Models.Reservations;
using EazyMenu.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EazyMenu.Web.Controllers;

[Authorize(Policy = "StaffAccess")]
public sealed class ReservationsController : Controller
{
    private readonly ILogger<ReservationsController> _logger;
    private readonly IDashboardTenantProvider _tenantProvider;
    private readonly IQueryHandler<GetReservationsForDayQuery, IReadOnlyCollection<ReservationSummaryDto>> _getReservationsHandler;
    private readonly IQueryHandler<GetBranchTablesQuery, IReadOnlyCollection<TableDto>> _getBranchTablesHandler;
    private readonly ICommandHandler<ScheduleReservationCommand, ReservationId> _scheduleReservationHandler;
    private readonly ICommandHandler<ConfirmReservationCommand, bool> _confirmReservationHandler;
    private readonly ICommandHandler<CancelReservationCommand, bool> _cancelReservationHandler;
    private readonly ICommandHandler<CheckInReservationCommand, bool> _checkInReservationHandler;
    private readonly ICommandHandler<MarkReservationNoShowCommand, bool> _markNoShowHandler;

    public ReservationsController(
        ILogger<ReservationsController> logger,
        IDashboardTenantProvider tenantProvider,
        IQueryHandler<GetReservationsForDayQuery, IReadOnlyCollection<ReservationSummaryDto>> getReservationsHandler,
        IQueryHandler<GetBranchTablesQuery, IReadOnlyCollection<TableDto>> getBranchTablesHandler,
        ICommandHandler<ScheduleReservationCommand, ReservationId> scheduleReservationHandler,
        ICommandHandler<ConfirmReservationCommand, bool> confirmReservationHandler,
        ICommandHandler<CancelReservationCommand, bool> cancelReservationHandler,
        ICommandHandler<CheckInReservationCommand, bool> checkInReservationHandler,
        ICommandHandler<MarkReservationNoShowCommand, bool> markNoShowHandler)
    {
        _logger = logger;
        _tenantProvider = tenantProvider;
        _getReservationsHandler = getReservationsHandler;
        _getBranchTablesHandler = getBranchTablesHandler;
        _scheduleReservationHandler = scheduleReservationHandler;
        _confirmReservationHandler = confirmReservationHandler;
        _cancelReservationHandler = cancelReservationHandler;
        _checkInReservationHandler = checkInReservationHandler;
        _markNoShowHandler = markNoShowHandler;
    }

    /// <summary>
    /// Helper method to get current tenant and its default branch
    /// </summary>
    private async Task<(Guid? tenantId, Guid? branchId)> GetTenantAndBranchAsync(CancellationToken cancellationToken)
    {
        var tenantId = await _tenantProvider.GetActiveTenantIdAsync(cancellationToken);
        if (!tenantId.HasValue)
        {
            return (null, null);
        }

        var branchId = await _tenantProvider.GetDefaultBranchIdAsync(tenantId.Value, cancellationToken);
        return (tenantId, branchId);
    }

    [HttpGet]
    public async Task<IActionResult> Index(DayOfWeek? dayOfWeek, CancellationToken cancellationToken)
    {
        var (tenantId, branchId) = await GetTenantAndBranchAsync(cancellationToken);
        if (!tenantId.HasValue)
        {
            TempData["ReservationError"] = "برای مشاهده رزروها ابتدا باید رستوران فعال انتخاب شود.";
            return RedirectToAction("Index", "Dashboard");
        }

        if (!branchId.HasValue)
        {
            TempData["ReservationError"] = "هیچ شعبه‌ای برای این رستوران تعریف نشده است. لطفاً ابتدا یک شعبه ایجاد کنید.";
            return RedirectToAction("Index", "Dashboard");
        }

        var selectedDay = dayOfWeek ?? DateTime.Today.DayOfWeek;
        var query = new GetReservationsForDayQuery(tenantId.Value, branchId.Value, selectedDay);

        try
        {
            var reservations = await _getReservationsHandler.HandleAsync(query, cancellationToken);
            var model = new ReservationListViewModel(
                tenantId.Value,
                branchId.Value,
                selectedDay,
                reservations.Select(r => new ReservationSummaryViewModel(
                    r.ReservationId,
                    r.TableId,
                    $"{r.Start:hh\\:mm} - {r.End:hh\\:mm}",
                    r.PartySize,
                    r.CustomerName ?? "مهمان",
                    r.Status,
                    r.SpecialRequest
                )).ToList()
            );

            ViewData["Title"] = "مدیریت رزرو میزها";
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در دریافت لیست رزروها");
            TempData["ReservationError"] = "در بازیابی رزروها خطایی رخ داد.";
            return RedirectToAction("Index", "Dashboard");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var (tenantId, branchId) = await GetTenantAndBranchAsync(cancellationToken);
        if (!tenantId.HasValue)
        {
            TempData["ErrorMessage"] = "برای ثبت رزرو ابتدا باید رستوران فعال انتخاب شود.";
            return RedirectToAction("Index", "Dashboard");
        }

        if (!branchId.HasValue)
        {
            TempData["ErrorMessage"] = "هیچ شعبه‌ای برای این رستوران تعریف نشده است.";
            return RedirectToAction("Index", "Dashboard");
        }

        var model = new CreateReservationViewModel
        {
            TenantId = tenantId.Value,
            BranchId = branchId.Value
        };

        ViewData["Title"] = "رزرو جدید";
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateReservationViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var startTime = TimeSpan.Parse(model.StartTime);
            var endTime = TimeSpan.Parse(model.EndTime);

            var command = new ScheduleReservationCommand(
                model.TenantId,
                model.BranchId,
                model.DayOfWeek,
                startTime,
                endTime,
                model.PartySize,
                model.PrefersOutdoor,
                model.SpecialRequest,
                model.CustomerName,
                model.CustomerPhone
            );

            var reservationId = await _scheduleReservationHandler.HandleAsync(command, cancellationToken);

            TempData["ReservationSuccess"] = "رزرو با موفقیت ثبت شد.";
            return RedirectToAction("Index", new { dayOfWeek = model.DayOfWeek });
        }
        catch (BusinessRuleViolationException ex)
        {
            _logger.LogWarning(ex, "خطای قانون کسب‌وکار در ثبت رزرو");
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطای غیرمنتظره در ثبت رزرو");
            ModelState.AddModelError(string.Empty, "در ثبت رزرو خطایی رخ داد. لطفاً دوباره تلاش کنید.");
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(Guid reservationId, string? note, CancellationToken cancellationToken)
    {
        var (tenantId, branchId) = await GetTenantAndBranchAsync(cancellationToken);
        if (!tenantId.HasValue || !branchId.HasValue)
        {
            return BadRequest(new { success = false, message = "رستوران یا شعبه فعال یافت نشد." });
        }

        try
        {
            var command = new ConfirmReservationCommand(reservationId, tenantId.Value, branchId.Value, note);
            await _confirmReservationHandler.HandleAsync(command, cancellationToken);

            return Ok(new { success = true, message = "رزرو با موفقیت تایید شد." });
        }
        catch (NotFoundException)
        {
            return NotFound(new { message = "رزرو مورد نظر یافت نشد." });
        }
        catch (BusinessRuleViolationException ex)
        {
            return UnprocessableEntity(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطای غیرمنتظره در تایید رزرو {ReservationId}", reservationId);
            return StatusCode(500, new { message = "در تایید رزرو خطایی رخ داد." });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid reservationId, string reason, CancellationToken cancellationToken)
    {
        var (tenantId, branchId) = await GetTenantAndBranchAsync(cancellationToken);
        if (!tenantId.HasValue || !branchId.HasValue)
        {
            return BadRequest(new { success = false, message = "رستوران یا شعبه فعال یافت نشد." });
        }

        try
        {
            var command = new CancelReservationCommand(reservationId, tenantId.Value, branchId.Value, reason);
            await _cancelReservationHandler.HandleAsync(command, cancellationToken);

            return Ok(new { success = true, message = "رزرو با موفقیت لغو شد." });
        }
        catch (NotFoundException)
        {
            return NotFound(new { message = "رزرو مورد نظر یافت نشد." });
        }
        catch (BusinessRuleViolationException ex)
        {
            return UnprocessableEntity(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطای غیرمنتظره در لغو رزرو {ReservationId}", reservationId);
            return StatusCode(500, new { message = "در لغو رزرو خطایی رخ داد." });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn(Guid reservationId, string? note, CancellationToken cancellationToken)
    {
        var (tenantId, branchId) = await GetTenantAndBranchAsync(cancellationToken);
        if (!tenantId.HasValue || !branchId.HasValue)
        {
            return BadRequest(new { success = false, message = "رستوران یا شعبه فعال یافت نشد." });
        }

        try
        {
            var command = new CheckInReservationCommand(reservationId, tenantId.Value, branchId.Value, note);
            await _checkInReservationHandler.HandleAsync(command, cancellationToken);

            return Ok(new { success = true, message = "مشتری با موفقیت چک‌این شد." });
        }
        catch (NotFoundException)
        {
            return NotFound(new { message = "رزرو مورد نظر یافت نشد." });
        }
        catch (BusinessRuleViolationException ex)
        {
            return UnprocessableEntity(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطای غیرمنتظره در چک‌این رزرو {ReservationId}", reservationId);
            return StatusCode(500, new { message = "در چک‌این رزرو خطایی رخ داد." });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkNoShow(Guid reservationId, string? note, CancellationToken cancellationToken)
    {
        var (tenantId, branchId) = await GetTenantAndBranchAsync(cancellationToken);
        if (!tenantId.HasValue || !branchId.HasValue)
        {
            return BadRequest(new { success = false, message = "رستوران یا شعبه فعال یافت نشد." });
        }

        try
        {
            var command = new MarkReservationNoShowCommand(reservationId, tenantId.Value, branchId.Value, note);
            await _markNoShowHandler.HandleAsync(command, cancellationToken);

            return Ok(new { success = true, message = "رزرو به عنوان عدم‌حضور ثبت شد." });
        }
        catch (NotFoundException)
        {
            return NotFound(new { message = "رزرو مورد نظر یافت نشد." });
        }
        catch (BusinessRuleViolationException ex)
        {
            return UnprocessableEntity(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطای غیرمنتظره در ثبت عدم‌حضور {ReservationId}", reservationId);
            return StatusCode(500, new { message = "در ثبت عدم‌حضور خطایی رخ داد." });
        }
    }
}
