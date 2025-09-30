using System;
using System.Collections.Generic;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Features.Menus.Common;

namespace EazyMenu.Application.Features.Menus.Commands.AddMenuItem;

public sealed record AddMenuItemCommand(
    Guid TenantId,
    Guid MenuId,
    Guid CategoryId,
    IDictionary<string, string> Name,
    IDictionary<string, string>? Description,
    decimal BasePrice,
    string? Currency,
    bool IsAvailable,
    InventoryPayload? Inventory,
    string? ImageUrl,
    IDictionary<string, decimal>? ChannelPrices,
    IReadOnlyCollection<string>? Tags) : ICommand<Guid>;
