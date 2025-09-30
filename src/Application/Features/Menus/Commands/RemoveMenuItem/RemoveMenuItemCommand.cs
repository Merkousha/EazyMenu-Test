using System;
using EazyMenu.Application.Abstractions.Messaging;

namespace EazyMenu.Application.Features.Menus.Commands.RemoveMenuItem;

public sealed record RemoveMenuItemCommand(
    Guid TenantId,
    Guid MenuId,
    Guid CategoryId,
    Guid ItemId) : ICommand<bool>;
