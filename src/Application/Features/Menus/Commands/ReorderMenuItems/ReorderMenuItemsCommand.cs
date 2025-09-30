using System;
using System.Collections.Generic;
using EazyMenu.Application.Abstractions.Messaging;

namespace EazyMenu.Application.Features.Menus.Commands.ReorderMenuItems;

public sealed record ReorderMenuItemsCommand(
    Guid TenantId,
    Guid MenuId,
    Guid CategoryId,
    IReadOnlyList<Guid> OrderedItemIds) : ICommand<bool>;
