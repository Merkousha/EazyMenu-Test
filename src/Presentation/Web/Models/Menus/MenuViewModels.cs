using System;
using System.Collections.Generic;

namespace EazyMenu.Web.Models.Menus;

public sealed record MenuListViewModel(
    Guid TenantId,
    IReadOnlyCollection<MenuSummaryViewModel> Menus,
    bool IsEmpty);

public sealed record MenuSummaryViewModel(
    Guid MenuId,
    string Name,
    bool IsDefault,
    bool IsArchived,
    int CategoryCount,
    int ItemCount,
    int PublishedVersion,
    DateTime UpdatedAtUtc);

public sealed record MenuDetailsViewModel(
    Guid TenantId,
    Guid MenuId,
    IDictionary<string, string> Name,
    IDictionary<string, string>? Description,
    bool IsDefault,
    bool IsArchived,
    int PublishedVersion,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyCollection<MenuCategoryViewModel> Categories);

public sealed record MenuCategoryViewModel(
    Guid CategoryId,
    IDictionary<string, string> Name,
    int DisplayOrder,
    string? IconUrl,
    bool IsArchived,
    IReadOnlyCollection<MenuItemViewModel> Items);

public sealed record MenuItemViewModel(
    Guid ItemId,
    IDictionary<string, string> Name,
    IDictionary<string, string>? Description,
    decimal BasePrice,
    string Currency,
    bool IsAvailable,
    InventoryViewModel Inventory,
    string? ImageUrl,
    int DisplayOrder,
    IReadOnlyDictionary<string, decimal> ChannelPrices,
    IReadOnlyCollection<string> Tags);

public sealed record InventoryViewModel(
    string Mode,
    int? Quantity,
    int? Threshold,
    bool IsBelowThreshold);
