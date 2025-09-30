using System;
using System.Collections.Generic;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Features.Menus.Common;

namespace EazyMenu.Application.Features.Menus.Commands.QuickUpdateMenuItem;

public sealed record QuickUpdateMenuItemCommand(
    Guid TenantId,
    Guid MenuId,
    Guid CategoryId,
    Guid ItemId,
    decimal BasePrice,
    string? Currency,
    IDictionary<string, decimal>? ChannelPrices,
    InventoryPayload? Inventory,
    bool IsAvailable) : ICommand<bool>;
