using System;
using System.Collections.Generic;

namespace EazyMenu.Application.Features.Menus.Common;

public sealed record PublishedMenuDto(
    Guid TenantId,
    Guid MenuId,
    int Version,
    Dictionary<string, string> Name,
    Dictionary<string, string>? Description,
    DateTime PublishedAtUtc,
    IReadOnlyCollection<PublishedMenuCategoryDto> Categories);

public sealed record PublishedMenuCategoryDto(
    Guid CategoryId,
    Dictionary<string, string> Name,
    string? IconUrl,
    int DisplayOrder,
    IReadOnlyCollection<PublishedMenuItemDto> Items);

public sealed record PublishedMenuItemDto(
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
