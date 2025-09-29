using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Features.Reservations.Common;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Reservations.GetBranchTables;

public sealed class GetBranchTablesQueryHandler : IQueryHandler<GetBranchTablesQuery, IReadOnlyCollection<TableDto>>
{
    private readonly IBranchRepository _branchRepository;

    public GetBranchTablesQueryHandler(IBranchRepository branchRepository)
    {
        _branchRepository = branchRepository;
    }

    public async Task<IReadOnlyCollection<TableDto>> HandleAsync(GetBranchTablesQuery query, CancellationToken cancellationToken = default)
    {
        if (!TenantId.TryCreate(query.TenantId, out var tenantId))
        {
            throw new BusinessRuleViolationException("شناسه مستاجر معتبر نیست.");
        }

        if (!BranchId.TryCreate(query.BranchId, out var branchId))
        {
            throw new BusinessRuleViolationException("شناسه شعبه معتبر نیست.");
        }

        var branch = await _branchRepository.GetByIdAsync(tenantId, branchId, cancellationToken);
        if (branch is null)
        {
            throw new NotFoundException("شعبه مورد نظر یافت نشد.");
        }

        return branch.Tables
            .Select(table => new TableDto(
                table.Id.Value,
                table.Label,
                table.Capacity,
                table.IsOutdoor,
                table.IsOutOfService))
            .ToList();
    }
}
