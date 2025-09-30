using System.Collections.Generic;
using System.Linq;
using EazyMenu.Domain.Aggregates.Menus;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Menus.Common;

internal static class MenuMapper
{
    public static MenuSummaryDto ToSummaryDto(Menu menu)
    {
        var allItems = menu.Categories.SelectMany(c => c.Items);
        return new MenuSummaryDto(
            menu.Id.Value,
            menu.Name.GetValueOrDefault(LocalizedText.DefaultCulture, LocalizedText.DefaultCulture),
            menu.IsDefault,
            menu.IsArchived,
            menu.Categories.Count,
            allItems.Count(),
            menu.PublishedVersion,
            menu.UpdatedAtUtc);
    }

    public static MenuDetailsDto ToDetailsDto(Menu menu, bool includeArchivedCategories = true)
    {
        var categories = menu.Categories
            .Where(category => includeArchivedCategories || !category.IsArchived)
            .OrderBy(category => category.DisplayOrder)
            .Select(ToCategoryDto)
            .ToList();

        return new MenuDetailsDto(
            menu.Id.Value,
            LocalizedTextMapper.ToDictionary(menu.Name),
            LocalizedTextMapper.ToDictionaryOrNull(menu.Description),
            menu.IsDefault,
            menu.IsArchived,
            menu.PublishedVersion,
            menu.CreatedAtUtc,
            menu.UpdatedAtUtc,
            categories);
    }

    private static MenuCategoryDetailsDto ToCategoryDto(MenuCategory category)
    {
        var items = category.Items
            .OrderBy(item => item.DisplayOrder)
            .Select(ToItemDto)
            .ToList();

        return new MenuCategoryDetailsDto(
            category.Id.Value,
            LocalizedTextMapper.ToDictionary(category.Name),
            category.DisplayOrder,
            category.IconUrl,
            category.IsArchived,
            items);
    }

    private static MenuItemDetailsDto ToItemDto(MenuItem item)
    {
        var channelPrices = item.ChannelPrices.ToDictionary(
            pair => pair.Key.ToString(),
            pair => pair.Value.Amount);

        var tags = item.Tags
            .Select(tag => tag.ToString())
            .ToList();

        var inventoryDetails = new InventoryDetailsDto(
            item.Inventory.Mode.ToString(),
            item.Inventory.Quantity,
            item.Inventory.Threshold,
            item.Inventory.IsBelowThreshold);

        return new MenuItemDetailsDto(
            item.Id.Value,
            LocalizedTextMapper.ToDictionary(item.Name),
            LocalizedTextMapper.ToDictionaryOrNull(item.Description),
            item.BasePrice.Amount,
            item.BasePrice.Currency,
            item.IsAvailable,
            inventoryDetails,
            item.ImageUrl,
            item.DisplayOrder,
            channelPrices,
            tags);
    }
}
