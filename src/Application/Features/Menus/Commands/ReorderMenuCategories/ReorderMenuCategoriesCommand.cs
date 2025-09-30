using System;
using System.Collections.Generic;
using EazyMenu.Application.Abstractions.Messaging;

namespace EazyMenu.Application.Features.Menus.Commands.ReorderMenuCategories;

public sealed record ReorderMenuCategoriesCommand(
    Guid TenantId,
    Guid MenuId,
    IReadOnlyCollection<Guid> OrderedCategoryIds) : ICommand<bool>;
