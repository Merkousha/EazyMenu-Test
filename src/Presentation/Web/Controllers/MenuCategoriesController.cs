using System;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Features.Menus.Commands.AddMenuCategory;
using EazyMenu.Application.Features.Menus.Commands.ArchiveMenuCategory;
using EazyMenu.Application.Features.Menus.Commands.ReorderMenuCategories;
using EazyMenu.Application.Features.Menus.Commands.RestoreMenuCategory;
using EazyMenu.Application.Features.Menus.Commands.UpdateMenuCategory;
using EazyMenu.Application.Features.Menus.Common;
using EazyMenu.Application.Features.Menus.Queries.GetMenuDetails;
using EazyMenu.Web.Models.Menus;
using EazyMenu.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EazyMenu.Web.Controllers;

public sealed class MenuCategoriesController : MenuDashboardControllerBase<MenuCategoriesController>
{
    private readonly ICommandHandler<AddMenuCategoryCommand, Guid> _addMenuCategoryCommandHandler;
    private readonly ICommandHandler<UpdateMenuCategoryCommand, bool> _updateMenuCategoryCommandHandler;
    private readonly ICommandHandler<ArchiveMenuCategoryCommand, bool> _archiveMenuCategoryCommandHandler;
    private readonly ICommandHandler<RestoreMenuCategoryCommand, bool> _restoreMenuCategoryCommandHandler;
    private readonly ICommandHandler<ReorderMenuCategoriesCommand, bool> _reorderMenuCategoriesCommandHandler;

    public MenuCategoriesController(
        ILogger<MenuCategoriesController> logger,
        IDashboardTenantProvider tenantProvider,
        IQueryHandler<GetMenuDetailsQuery, MenuDetailsDto> getMenuDetailsQueryHandler,
        ICommandHandler<AddMenuCategoryCommand, Guid> addMenuCategoryCommandHandler,
        ICommandHandler<UpdateMenuCategoryCommand, bool> updateMenuCategoryCommandHandler,
        ICommandHandler<ArchiveMenuCategoryCommand, bool> archiveMenuCategoryCommandHandler,
        ICommandHandler<RestoreMenuCategoryCommand, bool> restoreMenuCategoryCommandHandler,
        ICommandHandler<ReorderMenuCategoriesCommand, bool> reorderMenuCategoriesCommandHandler)
        : base(logger, tenantProvider, getMenuDetailsQueryHandler)
    {
        _addMenuCategoryCommandHandler = addMenuCategoryCommandHandler;
        _updateMenuCategoryCommandHandler = updateMenuCategoryCommandHandler;
        _archiveMenuCategoryCommandHandler = archiveMenuCategoryCommandHandler;
        _restoreMenuCategoryCommandHandler = restoreMenuCategoryCommandHandler;
        _reorderMenuCategoriesCommandHandler = reorderMenuCategoriesCommandHandler;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Guid menuId, CreateMenuCategoryInput input, CancellationToken cancellationToken)
    {
        var tenantId = await TryGetTenantIdAsync(cancellationToken);
        if (!tenantId.HasValue)
        {
            return TenantMissingResult();
        }

        if (input is null)
        {
            ModelState.AddModelError(string.Empty, "داده‌های ارسال‌شده معتبر نیست.");
            return ValidationProblem(ModelState);
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            await _addMenuCategoryCommandHandler.HandleAsync(
                new AddMenuCategoryCommand(tenantId.Value, menuId, input.Name.ToDictionary(), input.IconUrl, input.DisplayOrder),
                cancellationToken);

            var details = await LoadMenuDetailsAsync(tenantId.Value, menuId, cancellationToken);
            return CreateCategoryListPartial(details);
        }
        catch (Exception exception)
        {
            return HandleMenuException(menuId, exception, "create-category");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(Guid menuId, Guid categoryId, UpdateMenuCategoryInput input, CancellationToken cancellationToken)
    {
        var tenantId = await TryGetTenantIdAsync(cancellationToken);
        if (!tenantId.HasValue)
        {
            return TenantMissingResult();
        }

        if (input is null)
        {
            ModelState.AddModelError(string.Empty, "داده‌های ارسال‌شده معتبر نیست.");
            return ValidationProblem(ModelState);
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            await _updateMenuCategoryCommandHandler.HandleAsync(
                new UpdateMenuCategoryCommand(tenantId.Value, menuId, categoryId, input.Name.ToDictionary(), input.DisplayOrder, input.IconUrl),
                cancellationToken);

            var details = await LoadMenuDetailsAsync(tenantId.Value, menuId, cancellationToken);
            return CreateCategoryListPartial(details);
        }
        catch (Exception exception)
        {
            return HandleMenuException(menuId, exception, "update-category");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Archive(Guid menuId, Guid categoryId, CancellationToken cancellationToken)
    {
        var tenantId = await TryGetTenantIdAsync(cancellationToken);
        if (!tenantId.HasValue)
        {
            return TenantMissingResult();
        }

        try
        {
            await _archiveMenuCategoryCommandHandler.HandleAsync(
                new ArchiveMenuCategoryCommand(tenantId.Value, menuId, categoryId),
                cancellationToken);

            var details = await LoadMenuDetailsAsync(tenantId.Value, menuId, cancellationToken);
            return CreateCategoryListPartial(details);
        }
        catch (Exception exception)
        {
            return HandleMenuException(menuId, exception, "archive-category");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(Guid menuId, Guid categoryId, CancellationToken cancellationToken)
    {
        var tenantId = await TryGetTenantIdAsync(cancellationToken);
        if (!tenantId.HasValue)
        {
            return TenantMissingResult();
        }

        try
        {
            await _restoreMenuCategoryCommandHandler.HandleAsync(
                new RestoreMenuCategoryCommand(tenantId.Value, menuId, categoryId),
                cancellationToken);

            var details = await LoadMenuDetailsAsync(tenantId.Value, menuId, cancellationToken);
            return CreateCategoryListPartial(details);
        }
        catch (Exception exception)
        {
            return HandleMenuException(menuId, exception, "restore-category");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reorder(Guid menuId, [FromBody] ReorderCategoriesInput input, CancellationToken cancellationToken)
    {
        var tenantId = await TryGetTenantIdAsync(cancellationToken);
        if (!tenantId.HasValue)
        {
            return TenantMissingResult();
        }

        if (input is null)
        {
            ModelState.AddModelError(string.Empty, "داده‌های ارسال‌شده معتبر نیست.");
            return ValidationProblem(ModelState);
        }

        if (!ModelState.IsValid || input.CategoryIds.Count == 0)
        {
            if (input.CategoryIds.Count == 0)
            {
                ModelState.AddModelError(nameof(input.CategoryIds), "ترتیب دسته‌ها باید شامل حداقل یک شناسه باشد.");
            }

            return ValidationProblem(ModelState);
        }

        try
        {
            await _reorderMenuCategoriesCommandHandler.HandleAsync(
                new ReorderMenuCategoriesCommand(tenantId.Value, menuId, input.CategoryIds),
                cancellationToken);

            var details = await LoadMenuDetailsAsync(tenantId.Value, menuId, cancellationToken);
            return CreateCategoryListPartial(details);
        }
        catch (Exception exception)
        {
            return HandleMenuException(menuId, exception, "reorder-categories");
        }
    }
}
