using System;
using EazyMenu.Application.Abstractions.Messaging;

namespace EazyMenu.Application.Features.Menus.Commands.SetMenuItemAvailability;

public sealed record SetMenuItemAvailabilityCommand(
    Guid TenantId,
    Guid MenuId,
    Guid CategoryId,
    Guid ItemId,
    bool IsAvailable) : ICommand<bool>;
