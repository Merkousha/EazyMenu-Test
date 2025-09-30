using System;
using EazyMenu.Application.Abstractions.Messaging;

namespace EazyMenu.Application.Features.Menus.Commands.RemoveMenuCategory;

public sealed record RemoveMenuCategoryCommand(Guid TenantId, Guid MenuId, Guid CategoryId) : ICommand<bool>;
