using System;
using System.Collections.Generic;
using EazyMenu.Application.Abstractions.Messaging;

namespace EazyMenu.Application.Features.Menus.Commands.UpdateMenuMetadata;

public sealed record UpdateMenuMetadataCommand(
    Guid TenantId,
    Guid MenuId,
    IDictionary<string, string> Name,
    IDictionary<string, string>? Description,
    bool IsDefault) : ICommand<bool>;
