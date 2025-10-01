using System;
using System.Collections.Generic;
using System.Linq;

namespace EazyMenu.Public.Models.Menus;

public sealed record PublicMenuPageViewModel(
    Guid TenantId,
    bool HasMenu,
    PublicMenuViewModel? Menu);

public sealed record PublicMenuViewModel(
    Guid TenantId,
    Guid MenuId,
    string DisplayName,
    int Version,
    DateTime PublishedAtUtc,
    IReadOnlyCollection<PublicMenuCategoryViewModel> Categories,
    bool HasContent);

public sealed record PublicMenuCategoryViewModel(
    Guid CategoryId,
    string DisplayName,
    string? IconUrl,
    IReadOnlyCollection<PublicMenuItemViewModel> Items,
    bool HasVisibleItems);

public sealed record PublicMenuItemViewModel(
    Guid ItemId,
    string DisplayName,
    string? Description,
    decimal BasePrice,
    string Currency,
    bool IsAvailable,
    string AvailabilityLabel,
    IReadOnlyDictionary<string, decimal> ChannelPrices,
    IReadOnlyCollection<string> Tags,
    string? ImageUrl);

public static class PublicMenuExtensions
{
    public static string FormatPrice(this decimal amount)
    {
        return string.Format("{0:N0}", amount);
    }

    public static IReadOnlyCollection<KeyValuePair<string, decimal>> OrderedChannels(this IReadOnlyDictionary<string, decimal> prices)
    {
        return prices
            .OrderBy(pair => pair.Key)
            .ToList();
    }
}
