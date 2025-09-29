using System;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Tenants.Branches.CreateBranch;

public sealed record CreateBranchCommand(
    Guid TenantId,
    string Name,
    string City,
    string Street,
    string PostalCode) : ICommand<BranchId>;
