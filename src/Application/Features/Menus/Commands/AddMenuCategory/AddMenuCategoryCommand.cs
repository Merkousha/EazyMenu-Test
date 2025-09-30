using System;
using System.Collections.Generic;
using EazyMenu.Application.Abstractions.Messaging;

namespace EazyMenu.Application.Features.Menus.Commands.AddMenuCategory;

public sealed record AddMenuCategoryCommand(
    Guid TenantId,
    Guid MenuId,
    IDictionary<string, string> Name,
    string? IconUrl,
    int? DisplayOrder) : ICommand<Guid>;
