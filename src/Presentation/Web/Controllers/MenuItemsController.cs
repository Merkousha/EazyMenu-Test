using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Features.Menus.Commands.AddMenuItem;
using EazyMenu.Application.Features.Menus.Commands.RemoveMenuItem;
using EazyMenu.Application.Features.Menus.Commands.ReorderMenuItems;
using EazyMenu.Application.Features.Menus.Commands.SetMenuItemAvailability;
using EazyMenu.Application.Features.Menus.Commands.UpdateMenuItemDetails;
using EazyMenu.Application.Features.Menus.Commands.UpdateMenuItemPricing;
using EazyMenu.Application.Features.Menus.Common;
using EazyMenu.Application.Features.Menus.Queries.GetMenuDetails;
using EazyMenu.Web.Models.Menus;
using EazyMenu.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EazyMenu.Web.Controllers;

public sealed class MenuItemsController : MenuDashboardControllerBase<MenuItemsController>
{
    private readonly ICommandHandler<AddMenuItemCommand, Guid> _addMenuItemCommandHandler;
    private readonly ICommandHandler<UpdateMenuItemDetailsCommand, bool> _updateMenuItemDetailsCommandHandler;
    private readonly ICommandHandler<UpdateMenuItemPricingCommand, bool> _updateMenuItemPricingCommandHandler;
    private readonly ICommandHandler<SetMenuItemAvailabilityCommand, bool> _setMenuItemAvailabilityCommandHandler;
    private readonly ICommandHandler<RemoveMenuItemCommand, bool> _removeMenuItemCommandHandler;
    private readonly ICommandHandler<ReorderMenuItemsCommand, bool> _reorderMenuItemsCommandHandler;

    public MenuItemsController(
        ILogger<MenuItemsController> logger,
        IDashboardTenantProvider tenantProvider,
        IQueryHandler<GetMenuDetailsQuery, MenuDetailsDto> getMenuDetailsQueryHandler,
        ICommandHandler<AddMenuItemCommand, Guid> addMenuItemCommandHandler,
        ICommandHandler<UpdateMenuItemDetailsCommand, bool> updateMenuItemDetailsCommandHandler,
        ICommandHandler<UpdateMenuItemPricingCommand, bool> updateMenuItemPricingCommandHandler,
        ICommandHandler<SetMenuItemAvailabilityCommand, bool> setMenuItemAvailabilityCommandHandler,
        ICommandHandler<RemoveMenuItemCommand, bool> removeMenuItemCommandHandler,
        ICommandHandler<ReorderMenuItemsCommand, bool> reorderMenuItemsCommandHandler)
        : base(logger, tenantProvider, getMenuDetailsQueryHandler)
    {
        _addMenuItemCommandHandler = addMenuItemCommandHandler;
        _updateMenuItemDetailsCommandHandler = updateMenuItemDetailsCommandHandler;
        _updateMenuItemPricingCommandHandler = updateMenuItemPricingCommandHandler;
        _setMenuItemAvailabilityCommandHandler = setMenuItemAvailabilityCommandHandler;
        _removeMenuItemCommandHandler = removeMenuItemCommandHandler;
        _reorderMenuItemsCommandHandler = reorderMenuItemsCommandHandler;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Guid menuId, Guid categoryId, CreateMenuItemInput input, CancellationToken cancellationToken)
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

        InventoryPayload? inventoryPayload = null;
        try
        {
            inventoryPayload = input.Inventory?.ToPayload();
        }
        catch (ValidationException validationException)
        {
            ModelState.AddModelError(nameof(input.Inventory), validationException.Message);
            return ValidationProblem(ModelState);
        }

        try
        {
            var tags = input.ParseTags();
            await _addMenuItemCommandHandler.HandleAsync(
                new AddMenuItemCommand(
                    tenantId.Value,
                    menuId,
                    categoryId,
                    input.Name.ToDictionary(),
                    input.Description?.ToDictionary(),
                    input.BasePrice,
                    input.Currency,
                    input.IsAvailable,
                    inventoryPayload,
                    input.ImageUrl,
                    input.ChannelPrices.ToDictionary(),
                    tags),
                cancellationToken);

            var details = await LoadMenuDetailsAsync(tenantId.Value, menuId, cancellationToken);
            return CreateCategoryListPartial(details);
        }
        catch (Exception exception)
        {
            return HandleMenuException(menuId, exception, "create-item");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(Guid menuId, Guid categoryId, Guid itemId, UpdateMenuItemInput input, CancellationToken cancellationToken)
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
            var tags = input.ParseTags();

            await _updateMenuItemDetailsCommandHandler.HandleAsync(
                new UpdateMenuItemDetailsCommand(
                    tenantId.Value,
                    menuId,
                    categoryId,
                    itemId,
                    input.Name.ToDictionary(),
                    input.Description?.ToDictionary(),
                    input.ImageUrl,
                    tags),
                cancellationToken);

            await _updateMenuItemPricingCommandHandler.HandleAsync(
                new UpdateMenuItemPricingCommand(
                    tenantId.Value,
                    menuId,
                    categoryId,
                    itemId,
                    input.BasePrice,
                    input.Currency,
                    input.ChannelPrices.ToDictionary()),
                cancellationToken);

            await _setMenuItemAvailabilityCommandHandler.HandleAsync(
                new SetMenuItemAvailabilityCommand(tenantId.Value, menuId, categoryId, itemId, input.IsAvailable),
                cancellationToken);

            var details = await LoadMenuDetailsAsync(tenantId.Value, menuId, cancellationToken);
            return CreateCategoryListPartial(details);
        }
        catch (Exception exception)
        {
            return HandleMenuException(menuId, exception, "update-item");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetAvailability(Guid menuId, Guid categoryId, Guid itemId, [FromBody] SetMenuItemAvailabilityInput input, CancellationToken cancellationToken)
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

        if (!ModelState.IsValid || !input.IsAvailable.HasValue)
        {
            ModelState.AddModelError(nameof(input.IsAvailable), "وضعیت فعال بودن آیتم مشخص نشده است.");
            return ValidationProblem(ModelState);
        }

        try
        {
            await _setMenuItemAvailabilityCommandHandler.HandleAsync(
                new SetMenuItemAvailabilityCommand(tenantId.Value, menuId, categoryId, itemId, input.IsAvailable.Value),
                cancellationToken);

            var details = await LoadMenuDetailsAsync(tenantId.Value, menuId, cancellationToken);
            return CreateCategoryListPartial(details);
        }
        catch (Exception exception)
        {
            return HandleMenuException(menuId, exception, "set-item-availability");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(Guid menuId, Guid categoryId, Guid itemId, CancellationToken cancellationToken)
    {
        var tenantId = await TryGetTenantIdAsync(cancellationToken);
        if (!tenantId.HasValue)
        {
            return TenantMissingResult();
        }

        try
        {
            await _removeMenuItemCommandHandler.HandleAsync(
                new RemoveMenuItemCommand(tenantId.Value, menuId, categoryId, itemId),
                cancellationToken);

            var details = await LoadMenuDetailsAsync(tenantId.Value, menuId, cancellationToken);
            return CreateCategoryListPartial(details);
        }
        catch (Exception exception)
        {
            return HandleMenuException(menuId, exception, "remove-item");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reorder(Guid menuId, Guid categoryId, [FromBody] ReorderItemsInput input, CancellationToken cancellationToken)
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

        if (!ModelState.IsValid || input.ItemIds.Count == 0)
        {
            if (input.ItemIds.Count == 0)
            {
                ModelState.AddModelError(nameof(input.ItemIds), "ترتیب آیتم‌ها باید شامل حداقل یک شناسه باشد.");
            }

            return ValidationProblem(ModelState);
        }

        try
        {
            await _reorderMenuItemsCommandHandler.HandleAsync(
                new ReorderMenuItemsCommand(tenantId.Value, menuId, categoryId, input.ItemIds),
                cancellationToken);

            var details = await LoadMenuDetailsAsync(tenantId.Value, menuId, cancellationToken);
            return CreateCategoryListPartial(details);
        }
        catch (Exception exception)
        {
            return HandleMenuException(menuId, exception, "reorder-items");
        }
    }
}
