using System;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Features.Orders.CancelOrder;
using EazyMenu.Application.Features.Orders.CompleteOrder;
using EazyMenu.Application.Features.Orders.ConfirmOrder;
using EazyMenu.Application.Features.Orders.GetOrderDetails;
using EazyMenu.Application.Features.Orders.GetOrders;
using EazyMenu.Web.Services.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EazyMenu.Web.Controllers;

[Authorize(Policy = "StaffAccess")]
public sealed class OrdersController : Controller
{
    private readonly IQueryHandler<GetOrdersQuery, IReadOnlyCollection<Application.Features.Orders.Common.OrderSummaryDto>> _getOrdersHandler;
    private readonly IQueryHandler<GetOrderDetailsQuery, Application.Features.Orders.Common.OrderDetailsDto> _getOrderDetailsHandler;
    private readonly ICommandHandler<ConfirmOrderCommand, bool> _confirmOrderHandler;
    private readonly ICommandHandler<CompleteOrderCommand, bool> _completeOrderHandler;
    private readonly ICommandHandler<CancelOrderCommand, bool> _cancelOrderHandler;
    private readonly DashboardOrderViewModelFactory _viewModelFactory;
    private readonly ILogger<OrdersController> _logger;

    // TODO: Replace with actual tenant resolution from auth/session
    private static readonly Guid DemoTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public OrdersController(
        IQueryHandler<GetOrdersQuery, IReadOnlyCollection<Application.Features.Orders.Common.OrderSummaryDto>> getOrdersHandler,
        IQueryHandler<GetOrderDetailsQuery, Application.Features.Orders.Common.OrderDetailsDto> getOrderDetailsHandler,
        ICommandHandler<ConfirmOrderCommand, bool> confirmOrderHandler,
        ICommandHandler<CompleteOrderCommand, bool> completeOrderHandler,
        ICommandHandler<CancelOrderCommand, bool> cancelOrderHandler,
        DashboardOrderViewModelFactory viewModelFactory,
        ILogger<OrdersController> logger)
    {
        _getOrdersHandler = getOrdersHandler;
        _getOrderDetailsHandler = getOrderDetailsHandler;
        _confirmOrderHandler = confirmOrderHandler;
        _completeOrderHandler = completeOrderHandler;
        _cancelOrderHandler = cancelOrderHandler;
        _viewModelFactory = viewModelFactory;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? status = null, int page = 1)
    {
        try
        {
            var query = new GetOrdersQuery(DemoTenantId, status, page, 20);
            var orders = await _getOrdersHandler.HandleAsync(query);
            
            var viewModel = _viewModelFactory.CreateListViewModel(orders, page, 20, status);
            
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در دریافت لیست سفارش‌ها");
            TempData["Error"] = "خطا در دریافت سفارش‌ها. لطفاً دوباره تلاش کنید.";
            return View(new ViewModels.Orders.OrderListViewModel());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        try
        {
            var query = new GetOrderDetailsQuery(DemoTenantId, id);
            var order = await _getOrderDetailsHandler.HandleAsync(query);
            
            var viewModel = _viewModelFactory.ToDetailsViewModel(order);
            
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در دریافت جزئیات سفارش {OrderId}", id);
            TempData["Error"] = "سفارش مورد نظر یافت نشد.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(Guid id)
    {
        try
        {
            var command = new ConfirmOrderCommand(DemoTenantId, id);
            await _confirmOrderHandler.HandleAsync(command);
            
            TempData["Success"] = "سفارش با موفقیت تأیید شد.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در تأیید سفارش {OrderId}", id);
            TempData["Error"] = "خطا در تأیید سفارش. لطفاً دوباره تلاش کنید.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(Guid id)
    {
        try
        {
            var command = new CompleteOrderCommand(DemoTenantId, id);
            await _completeOrderHandler.HandleAsync(command);
            
            TempData["Success"] = "سفارش با موفقیت تکمیل شد.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در تکمیل سفارش {OrderId}", id);
            TempData["Error"] = "خطا در تکمیل سفارش. لطفاً دوباره تلاش کنید.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid id, string? reason = null)
    {
        try
        {
            var command = new CancelOrderCommand(DemoTenantId, id, reason);
            await _cancelOrderHandler.HandleAsync(command);
            
            TempData["Success"] = "سفارش با موفقیت لغو شد.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در لغو سفارش {OrderId}", id);
            TempData["Error"] = "خطا در لغو سفارش. لطفاً دوباره تلاش کنید.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
