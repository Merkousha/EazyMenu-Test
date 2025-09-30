using System;
using System.Collections.Generic;
using EazyMenu.Application.Abstractions.Messaging;

namespace EazyMenu.Application.Features.Menus.Commands.UpdateMenuItemPricing;

public sealed record UpdateMenuItemPricingCommand(
    Guid TenantId,
    Guid MenuId,
    Guid CategoryId,
    Guid ItemId,
    decimal BasePrice,
    string? Currency,
    IDictionary<string, decimal>? ChannelPrices) : ICommand<bool>;
