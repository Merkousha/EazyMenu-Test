using System;
using System.Collections.Generic;
using System.Linq;
using EazyMenu.Application.Features.Menus.Common;

namespace EazyMenu.Web.Models.Menus;

public static class MenuViewModelFactory
{
    public static MenuListViewModel CreateMenuList(Guid tenantId, IReadOnlyCollection<MenuSummaryDto> summaries)
    {
        var items = summaries
            .OrderByDescending(menu => menu.IsDefault)
            .ThenBy(menu => menu.Name)
            .Select(MapSummary)
            .ToList();

        return new MenuListViewModel(tenantId, items, items.Count == 0);
    }

    public static MenuDetailsViewModel CreateMenuDetails(Guid tenantId, MenuDetailsDto dto)
    {
        var categories = dto.Categories
            .OrderBy(category => category.DisplayOrder)
            .Select(MapCategory)
            .ToList();

        return new MenuDetailsViewModel(
            tenantId,
            dto.MenuId,
            new Dictionary<string, string>(dto.Name),
            dto.Description is null ? null : new Dictionary<string, string>(dto.Description),
            dto.IsDefault,
            dto.IsArchived,
            dto.PublishedVersion,
            dto.CreatedAtUtc,
            dto.UpdatedAtUtc,
            categories);
    }

    public static MenuCategoryViewModel MapCategory(MenuCategoryDetailsDto dto)
    {
        var items = dto.Items
            .OrderBy(item => item.DisplayOrder)
            .Select(MapItem)
            .ToList();

        return new MenuCategoryViewModel(
            dto.CategoryId,
            new Dictionary<string, string>(dto.Name),
            dto.DisplayOrder,
            dto.IconUrl,
            dto.IsArchived,
            items);
    }

    public static MenuItemViewModel MapItem(MenuItemDetailsDto dto)
    {
        var inventory = new InventoryViewModel(dto.Inventory.Mode, dto.Inventory.Quantity, dto.Inventory.Threshold, dto.Inventory.IsBelowThreshold);

        return new MenuItemViewModel(
            dto.ItemId,
            new Dictionary<string, string>(dto.Name),
            dto.Description is null ? null : new Dictionary<string, string>(dto.Description),
            dto.BasePrice,
            dto.Currency,
            dto.IsAvailable,
            inventory,
            dto.ImageUrl,
            dto.DisplayOrder,
            new Dictionary<string, decimal>(dto.ChannelPrices),
            dto.Tags.ToList());
    }

    private static MenuSummaryViewModel MapSummary(MenuSummaryDto dto)
    {
        return new MenuSummaryViewModel(
            dto.MenuId,
            dto.Name,
            dto.IsDefault,
            dto.IsArchived,
            dto.CategoryCount,
            dto.ItemCount,
            dto.PublishedVersion,
            dto.UpdatedAtUtc);
    }
}
