using System;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Domain.Common.Exceptions;
using EazyMenu.Application.Features.Menus.Common;
using EazyMenu.Application.Features.Menus.Queries.GetMenuDetails;
using EazyMenu.Web.Models.Menus;
using EazyMenu.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EazyMenu.Web.Controllers;

[Authorize(Policy = "StaffAccess")]
public abstract class MenuDashboardControllerBase<TController> : Controller
{
    private readonly IDashboardTenantProvider _tenantProvider;
    private readonly IQueryHandler<GetMenuDetailsQuery, MenuDetailsDto> _getMenuDetailsQueryHandler;

    protected MenuDashboardControllerBase(
        ILogger<TController> logger,
        IDashboardTenantProvider tenantProvider,
        IQueryHandler<GetMenuDetailsQuery, MenuDetailsDto> getMenuDetailsQueryHandler)
    {
        Logger = logger;
        _tenantProvider = tenantProvider;
        _getMenuDetailsQueryHandler = getMenuDetailsQueryHandler;
    }

    protected ILogger<TController> Logger { get; }

    protected Task<Guid?> TryGetTenantIdAsync(CancellationToken cancellationToken)
    {
        return _tenantProvider.GetActiveTenantIdAsync(cancellationToken);
    }

    protected Task<MenuDetailsViewModel> LoadMenuDetailsAsync(Guid tenantId, Guid menuId, CancellationToken cancellationToken)
    {
        return LoadMenuDetailsAsync(tenantId, menuId, includeArchivedCategories: true, cancellationToken: cancellationToken);
    }

    protected async Task<MenuDetailsViewModel> LoadMenuDetailsAsync(Guid tenantId, Guid menuId, bool includeArchivedCategories, CancellationToken cancellationToken)
    {
        var dto = await _getMenuDetailsQueryHandler.HandleAsync(
            new GetMenuDetailsQuery(tenantId, menuId, includeArchivedCategories),
            cancellationToken);

        return MenuViewModelFactory.CreateMenuDetails(tenantId, dto);
    }

    protected PartialViewResult CreateCategoryListPartial(MenuDetailsViewModel detailsViewModel)
    {
        ViewData["MenuId"] = detailsViewModel.MenuId;
        ViewData["TenantId"] = detailsViewModel.TenantId;
        return PartialView("~/Views/Menus/Partials/_CategoryList.cshtml", detailsViewModel.Categories);
    }

    protected IActionResult TenantMissingResult()
    {
        return BadRequest(new { message = "برای مدیریت منو ابتدا باید مستاجر فعال مشخص شود." });
    }

    protected PartialViewResult CreateQuickUpdatePartial(MenuDetailsViewModel detailsViewModel)
    {
        ViewData["MenuId"] = detailsViewModel.MenuId;
        ViewData["TenantId"] = detailsViewModel.TenantId;
        return PartialView("~/Views/Menus/Partials/_QuickUpdateTable.cshtml", detailsViewModel);
    }

    protected IActionResult HandleMenuException(Guid menuId, Exception exception, string context)
    {
        switch (exception)
        {
            case NotFoundException:
                Logger.LogWarning(exception, "Menu {MenuId} not found during {Context}", menuId, context);
                return NotFound(new { message = "منوی درخواستی یافت نشد." });
            case BusinessRuleViolationException businessRuleViolation:
                Logger.LogWarning(exception, "Business rule violation for menu {MenuId} during {Context}", menuId, context);
                return UnprocessableEntity(new { message = businessRuleViolation.Message });
            default:
                Logger.LogError(exception, "Unexpected error for menu {MenuId} during {Context}", menuId, context);
                return StatusCode(500, new { message = "خطای غیرمنتظره‌ای رخ داد. لطفاً دوباره تلاش کنید." });
        }
    }
}
