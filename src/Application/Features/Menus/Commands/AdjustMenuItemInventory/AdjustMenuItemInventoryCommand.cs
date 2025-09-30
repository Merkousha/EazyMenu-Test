using System;
using EazyMenu.Application.Abstractions.Messaging;

namespace EazyMenu.Application.Features.Menus.Commands.AdjustMenuItemInventory;

public sealed record AdjustMenuItemInventoryCommand(
    Guid TenantId,
    Guid MenuId,
    Guid CategoryId,
    Guid ItemId,
    int Delta) : ICommand<bool>;
