using System;
using System.Collections.Generic;
using System.Linq;
using EazyMenu.Domain.Aggregates.Menus;

namespace EazyMenu.Application.Features.Menus.Common;

public static class MenuPublicationFactory
{
    public static PublishedMenuDto CreateSnapshot(Menu menu, DateTime publishedAtUtc)
    {
        var categories = menu.Categories
            .Where(category => !category.IsArchived)
            .OrderBy(category => category.DisplayOrder)
            .Select(category => new PublishedMenuCategoryDto(
                category.Id.Value,
                LocalizedTextMapper.ToDictionary(category.Name),
                category.IconUrl,
                category.DisplayOrder,
                category.Items
                    .OrderBy(item => item.DisplayOrder)
                    .Select(item => new PublishedMenuItemDto(
                        item.Id.Value,
                        LocalizedTextMapper.ToDictionary(item.Name),
                        LocalizedTextMapper.ToDictionaryOrNull(item.Description),
                        item.BasePrice.Amount,
                        item.BasePrice.Currency,
                        item.IsAvailable,
                        new InventoryDetailsDto(
                            item.Inventory.Mode.ToString(),
                            item.Inventory.Quantity,
                            item.Inventory.Threshold,
                            item.Inventory.IsBelowThreshold),
                        item.ImageUrl,
                        item.DisplayOrder,
                        item.ChannelPrices.ToDictionary(
                            pair => pair.Key.ToString(),
                            pair => pair.Value.Amount),
                        item.Tags
                            .Select(tag => tag.ToString())
                            .ToList()))
                    .ToList()))
            .ToList();

        return new PublishedMenuDto(
            menu.TenantId.Value,
            menu.Id.Value,
            menu.PublishedVersion,
            LocalizedTextMapper.ToDictionary(menu.Name),
            LocalizedTextMapper.ToDictionaryOrNull(menu.Description),
            publishedAtUtc,
            categories);
    }
}
