using System;
using System.Collections.Generic;
using EazyMenu.Application.Abstractions.Messaging;

namespace EazyMenu.Application.Features.Menus.Commands.UpdateMenuItemDetails;

public sealed record UpdateMenuItemDetailsCommand(
    Guid TenantId,
    Guid MenuId,
    Guid CategoryId,
    Guid ItemId,
    IDictionary<string, string> Name,
    IDictionary<string, string>? Description,
    string? ImageUrl,
    IReadOnlyCollection<string>? Tags) : ICommand<bool>;
