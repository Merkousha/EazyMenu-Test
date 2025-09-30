using System;
using System.Collections.Generic;
using EazyMenu.Application.Abstractions.Messaging;

namespace EazyMenu.Application.Features.Menus.Commands.UpdateMenuCategory;

public sealed record UpdateMenuCategoryCommand(
    Guid TenantId,
    Guid MenuId,
    Guid CategoryId,
    IDictionary<string, string> Name,
    int DisplayOrder,
    string? IconUrl) : ICommand<bool>;
