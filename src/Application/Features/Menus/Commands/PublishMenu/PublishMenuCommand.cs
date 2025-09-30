using System;
using EazyMenu.Application.Abstractions.Messaging;

namespace EazyMenu.Application.Features.Menus.Commands.PublishMenu;

public sealed record PublishMenuCommand(
    Guid TenantId,
    Guid MenuId) : ICommand<int>;
