using System;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Tenants.Branches.Tables.AddTable;

public sealed record AddBranchTableCommand(
    Guid TenantId,
    Guid BranchId,
    string Label,
    int Capacity,
    bool IsOutdoor) : ICommand<TableId>;
