using System;
using System.Collections.Generic;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Features.Reservations.Common;

namespace EazyMenu.Application.Features.Reservations.GetBranchTables;

public sealed record GetBranchTablesQuery(Guid TenantId, Guid BranchId) : IQuery<IReadOnlyCollection<TableDto>>;
