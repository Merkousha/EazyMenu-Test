using System;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Features.Tenants.Common;

namespace EazyMenu.Application.Features.Tenants.Branches.GetBranchDetails;

public sealed record GetBranchDetailsQuery(Guid TenantId, Guid BranchId) : IQuery<BranchDetailsDto>;
