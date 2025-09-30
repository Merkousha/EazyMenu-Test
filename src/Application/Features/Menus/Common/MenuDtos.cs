using System;
using System.Collections.Generic;

namespace EazyMenu.Application.Features.Menus.Common;

public sealed record MenuSummaryDto(
    Guid MenuId,
    string Name,
    bool IsDefault,
    bool IsArchived,
    int CategoryCount,
    int ItemCount,
    int PublishedVersion,
    DateTime UpdatedAtUtc);

public sealed record MenuDetailsDto(
    Guid MenuId,
    Dictionary<string, string> Name,
    Dictionary<string, string>? Description,
    bool IsDefault,
    bool IsArchived,
    int PublishedVersion,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyCollection<MenuCategoryDetailsDto> Categories);

public sealed record MenuCategoryDetailsDto(
    Guid CategoryId,
    Dictionary<string, string> Name,
    int DisplayOrder,
    string? IconUrl,
    bool IsArchived,
    IReadOnlyCollection<MenuItemDetailsDto> Items);

public sealed record MenuItemDetailsDto(
    Guid ItemId,
    Dictionary<string, string> Name,
    Dictionary<string, string>? Description,
    decimal BasePrice,
    string Currency,
    bool IsAvailable,
    InventoryDetailsDto Inventory,
    string? ImageUrl,
    int DisplayOrder,
    IReadOnlyDictionary<string, decimal> ChannelPrices,
    IReadOnlyCollection<string> Tags);

public sealed record InventoryDetailsDto(
    string Mode,
    int? Quantity,
    int? Threshold,
    bool IsBelowThreshold);
