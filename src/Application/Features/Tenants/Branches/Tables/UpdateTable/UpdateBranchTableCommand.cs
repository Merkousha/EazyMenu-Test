using System;
using EazyMenu.Application.Abstractions.Messaging;

namespace EazyMenu.Application.Features.Tenants.Branches.Tables.UpdateTable;

public sealed record UpdateBranchTableCommand(
    Guid TenantId,
    Guid BranchId,
    Guid TableId,
    string? Label,
    int? Capacity,
    bool? IsOutdoor) : ICommand<bool>;
