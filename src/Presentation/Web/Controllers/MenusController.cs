using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Features.Menus.Commands.PublishMenu;
using EazyMenu.Application.Features.Menus.Common;
using EazyMenu.Application.Features.Menus.Queries.GetMenuDetails;
using EazyMenu.Application.Features.Menus.Queries.GetMenus;
using EazyMenu.Web.Models.Menus;
using EazyMenu.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EazyMenu.Web.Controllers;

public sealed class MenusController : Controller
{
    private readonly ILogger<MenusController> _logger;
    private readonly IDashboardTenantProvider _tenantProvider;
    private readonly IQueryHandler<GetMenusQuery, IReadOnlyCollection<MenuSummaryDto>> _getMenusQueryHandler;
    private readonly IQueryHandler<GetMenuDetailsQuery, MenuDetailsDto> _getMenuDetailsQueryHandler;
    private readonly ICommandHandler<PublishMenuCommand, int> _publishMenuCommandHandler;

    public MenusController(
        ILogger<MenusController> logger,
        IDashboardTenantProvider tenantProvider,
        IQueryHandler<GetMenusQuery, IReadOnlyCollection<MenuSummaryDto>> getMenusQueryHandler,
        IQueryHandler<GetMenuDetailsQuery, MenuDetailsDto> getMenuDetailsQueryHandler,
        ICommandHandler<PublishMenuCommand, int> publishMenuCommandHandler)
    {
        _logger = logger;
        _tenantProvider = tenantProvider;
        _getMenusQueryHandler = getMenusQueryHandler;
        _getMenuDetailsQueryHandler = getMenuDetailsQueryHandler;
        _publishMenuCommandHandler = publishMenuCommandHandler;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var tenantId = await _tenantProvider.GetActiveTenantIdAsync(cancellationToken);
        ViewData["Title"] = "مدیریت منوها";

        if (!tenantId.HasValue)
        {
            var emptyModel = new MenuListViewModel(Guid.Empty, Array.Empty<MenuSummaryViewModel>(), true);
            return View(emptyModel);
        }

        var menus = await _getMenusQueryHandler.HandleAsync(new GetMenusQuery(tenantId.Value), cancellationToken);
        var model = MenuViewModelFactory.CreateMenuList(tenantId.Value, menus);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid menuId, CancellationToken cancellationToken)
    {
        var tenantId = await _tenantProvider.GetActiveTenantIdAsync(cancellationToken);
        if (!tenantId.HasValue)
        {
            TempData["MenuError"] = "برای مشاهده منوها ابتدا باید مستاجر فعال ایجاد شود.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var dto = await _getMenuDetailsQueryHandler.HandleAsync(
                new GetMenuDetailsQuery(tenantId.Value, menuId, IncludeArchivedCategories: true),
                cancellationToken);

            ViewData["Title"] = dto.Name.TryGetValue("fa-IR", out var nameFa)
                ? $"ویرایش {nameFa}"
                : "جزئیات منو";

            var model = MenuViewModelFactory.CreateMenuDetails(tenantId.Value, dto);
            return View(model);
        }
        catch (NotFoundException)
        {
            TempData["MenuError"] = "منوی درخواستی یافت نشد.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleViolationException exception)
        {
            _logger.LogWarning(exception, "خطای اعتبارسنجی در دریافت جزئیات منو {MenuId}", menuId);
            TempData["MenuError"] = exception.Message;
            return RedirectToAction(nameof(Index));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "خطای غیرمنتظره هنگام دریافت منو {MenuId}", menuId);
            TempData["MenuError"] = "در بازیابی منوی درخواستی خطایی رخ داد.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(Guid menuId, CancellationToken cancellationToken)
    {
        var tenantId = await _tenantProvider.GetActiveTenantIdAsync(cancellationToken);
        if (!tenantId.HasValue)
        {
            return BadRequest(new { message = "برای انتشار منو ابتدا باید مستاجر فعال مشخص شود." });
        }

        try
        {
            var version = await _publishMenuCommandHandler.HandleAsync(
                new PublishMenuCommand(tenantId.Value, menuId),
                cancellationToken);

            return Ok(new { version });
        }
        catch (NotFoundException)
        {
            _logger.LogWarning("Menu {MenuId} was not found for publish operation", menuId);
            return NotFound(new { message = "منوی درخواستی یافت نشد." });
        }
        catch (BusinessRuleViolationException exception)
        {
            _logger.LogWarning(exception, "Business rule violation while publishing menu {MenuId}", menuId);
            return UnprocessableEntity(new { message = exception.Message });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error while publishing menu {MenuId}", menuId);
            return StatusCode(500, new { message = "خطای غیرمنتظره‌ای رخ داد. لطفاً دوباره تلاش کنید." });
        }
    }
}
