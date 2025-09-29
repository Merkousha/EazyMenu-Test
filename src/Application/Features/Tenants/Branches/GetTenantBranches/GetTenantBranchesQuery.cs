using System;
using System.Collections.Generic;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Features.Tenants.Common;

namespace EazyMenu.Application.Features.Tenants.Branches.GetTenantBranches;

public sealed record GetTenantBranchesQuery(Guid TenantId) : IQuery<IReadOnlyCollection<BranchSummaryDto>>;
