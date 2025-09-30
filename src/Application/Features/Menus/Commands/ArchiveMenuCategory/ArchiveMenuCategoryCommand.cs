using System;
using EazyMenu.Application.Abstractions.Messaging;

namespace EazyMenu.Application.Features.Menus.Commands.ArchiveMenuCategory;

public sealed record ArchiveMenuCategoryCommand(Guid TenantId, Guid MenuId, Guid CategoryId) : ICommand<bool>;
