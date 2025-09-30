using System;
using EazyMenu.Application.Abstractions.Messaging;

namespace EazyMenu.Application.Features.Menus.Commands.RestoreMenuCategory;

public sealed record RestoreMenuCategoryCommand(Guid TenantId, Guid MenuId, Guid CategoryId) : ICommand<bool>;
