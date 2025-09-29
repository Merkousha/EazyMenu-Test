using System;
using EazyMenu.Application.Abstractions.Messaging;

namespace EazyMenu.Application.Features.Tenants.Branches.UpdateBranchDetails;

public sealed record UpdateBranchDetailsCommand(
    Guid TenantId,
    Guid BranchId,
    string? Name,
    string? City,
    string? Street,
    string? PostalCode) : ICommand<bool>;
