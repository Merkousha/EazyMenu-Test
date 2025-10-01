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

    public static PublicMenuPageViewModel CreatePageModel(
        PublishedMenuDto? snapshot,
        Guid tenantId,
        string preferredCulture = "fa-IR",
        string? searchTerm = null)
    {
        var normalizedSearchTerm = NormalizeSearchTerm(searchTerm);

        if (snapshot is null)
        {
            return new PublicMenuPageViewModel(tenantId, false, null, normalizedSearchTerm, false);
        }

        var menu = Create(snapshot, preferredCulture);

        if (!string.IsNullOrEmpty(normalizedSearchTerm))
        {
            menu = ApplySearch(menu, normalizedSearchTerm);
        }

        var hasResults = menu.Categories.Any(category => category.Items.Any());

        return new PublicMenuPageViewModel(tenantId, true, menu, normalizedSearchTerm, hasResults);
    }

    private static PublicMenuViewModel ApplySearch(PublicMenuViewModel menu, string searchTerm)
    {
        var comparison = StringComparison.CurrentCultureIgnoreCase;

        var filteredCategories = menu.Categories
            .Select(category => FilterCategory(category, searchTerm, comparison))
            .Where(category => category.Items.Any())
            .ToList();

        var hasVisibleItems = filteredCategories.Any(category => category.HasVisibleItems);

        return menu with
        {
            Categories = filteredCategories,
            HasContent = hasVisibleItems
        };
    }

    private static PublicMenuCategoryViewModel FilterCategory(
        PublicMenuCategoryViewModel category,
        string searchTerm,
        StringComparison comparison)
    {
        var filteredItems = category.Items
            .Where(item => MatchesQuery(item, searchTerm, comparison))
            .ToList();

        return category with
        {
            Items = filteredItems,
            HasVisibleItems = filteredItems.Any(item => item.IsAvailable)
        };
    }

    private static bool MatchesQuery(PublicMenuItemViewModel item, string searchTerm, StringComparison comparison)
    {
        if (!string.IsNullOrWhiteSpace(item.DisplayName) && item.DisplayName.Contains(searchTerm, comparison))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(item.Description) && item.Description.Contains(searchTerm, comparison))
        {
            return true;
        }

        if (item.Tags.Any(tag => tag.Contains(searchTerm, comparison)))
        {
            return true;
        }

        return false;
    }

    private static string? NormalizeSearchTerm(string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return null;
        }

        return searchTerm.Trim();
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
