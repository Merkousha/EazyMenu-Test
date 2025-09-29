using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Features.Tenants.Common;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Tenants.Branches.GetBranchDetails;

public sealed class GetBranchDetailsQueryHandler : IQueryHandler<GetBranchDetailsQuery, BranchDetailsDto>
{
    private readonly IBranchRepository _branchRepository;

    public GetBranchDetailsQueryHandler(IBranchRepository branchRepository)
    {
        _branchRepository = branchRepository;
    }

    public async Task<BranchDetailsDto> HandleAsync(GetBranchDetailsQuery query, CancellationToken cancellationToken = default)
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

        var workingHours = branch.WorkingHours
            .OrderBy(slot => slot.DayOfWeek)
            .ThenBy(slot => slot.Start)
            .Select(slot => new WorkingHourDto(slot.DayOfWeek, slot.Start, slot.End))
            .ToList();

        var tables = branch.Tables
            .OrderBy(table => table.Label)
            .Select(table => new BranchTableDto(
                table.Id.Value,
                table.Label,
                table.Capacity,
                table.IsOutdoor,
                table.IsOutOfService))
            .ToList();

        return new BranchDetailsDto(
            branch.Id.Value,
            branch.Name,
            branch.Address.City,
            branch.Address.Street,
            branch.Address.PostalCode,
            workingHours,
            tables);
    }
}
