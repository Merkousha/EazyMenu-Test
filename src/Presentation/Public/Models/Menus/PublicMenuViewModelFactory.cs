using System;
using System.Collections.Generic;
using System.Linq;
using EazyMenu.Application.Features.Menus.Common;

namespace EazyMenu.Public.Models.Menus;

public static class PublicMenuViewModelFactory
{
    public static PublicMenuViewModel Create(PublishedMenuDto snapshot, string preferredCulture = "fa-IR")
    {
        var categories = snapshot.Categories
            .Select(category => MapCategory(category, preferredCulture))
            .ToList();

        return new PublicMenuViewModel(
            snapshot.TenantId,
            snapshot.MenuId,
            GetLocalizedValue(snapshot.Name, preferredCulture),
            snapshot.Version,
            snapshot.PublishedAtUtc,
            categories,
            categories.Any(category => category.HasVisibleItems));
    }

    public static PublicMenuPageViewModel CreatePageModel(PublishedMenuDto? snapshot, Guid tenantId, string preferredCulture = "fa-IR")
    {
        if (snapshot is null)
        {
            return new PublicMenuPageViewModel(tenantId, false, null);
        }

        var menu = Create(snapshot, preferredCulture);
        return new PublicMenuPageViewModel(tenantId, true, menu);
    }

    private static PublicMenuCategoryViewModel MapCategory(PublishedMenuCategoryDto category, string preferredCulture)
    {
        var items = category.Items
            .Select(item => MapItem(item, preferredCulture))
            .ToList();

        return new PublicMenuCategoryViewModel(
            category.CategoryId,
            GetLocalizedValue(category.Name, preferredCulture),
            category.IconUrl,
            items,
            items.Any(item => item.IsAvailable));
    }

    private static PublicMenuItemViewModel MapItem(PublishedMenuItemDto item, string preferredCulture)
    {
        var availabilityLabel = item.IsAvailable
            ? "موجود"
            : "ناموجود";

        return new PublicMenuItemViewModel(
            item.ItemId,
            GetLocalizedValue(item.Name, preferredCulture),
            GetLocalizedValueOrNull(item.Description, preferredCulture),
            item.BasePrice,
            item.Currency,
            item.IsAvailable,
            availabilityLabel,
            item.ChannelPrices,
            item.Tags,
            item.ImageUrl);
    }

    private static string GetLocalizedValue(IReadOnlyDictionary<string, string> values, string preferredCulture)
    {
        if (values.TryGetValue(preferredCulture, out var preferred) && !string.IsNullOrWhiteSpace(preferred))
        {
            return preferred;
        }

        return values.Values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;
    }

    private static string? GetLocalizedValueOrNull(IReadOnlyDictionary<string, string>? values, string preferredCulture)
    {
        if (values is null)
        {
            return null;
        }

        var localized = GetLocalizedValue(values, preferredCulture);
        return string.IsNullOrWhiteSpace(localized) ? null : localized;
    }
}
