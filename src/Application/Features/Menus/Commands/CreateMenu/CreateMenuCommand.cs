using System;
using System.Collections.Generic;
using EazyMenu.Application.Abstractions.Messaging;

namespace EazyMenu.Application.Features.Menus.Commands.CreateMenu;

public sealed record CreateMenuCommand(
    Guid TenantId,
    IDictionary<string, string> Name,
    IDictionary<string, string>? Description,
    bool IsDefault) : ICommand<Guid>;
