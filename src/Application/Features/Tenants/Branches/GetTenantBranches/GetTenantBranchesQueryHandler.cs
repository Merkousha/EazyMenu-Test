using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Features.Tenants.Common;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Tenants.Branches.GetTenantBranches;

public sealed class GetTenantBranchesQueryHandler : IQueryHandler<GetTenantBranchesQuery, IReadOnlyCollection<BranchSummaryDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetTenantBranchesQueryHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<IReadOnlyCollection<BranchSummaryDto>> HandleAsync(GetTenantBranchesQuery query, CancellationToken cancellationToken = default)
    {
        if (!TenantId.TryCreate(query.TenantId, out var tenantId))
        {
            throw new BusinessRuleViolationException("شناسه مستاجر معتبر نیست.");
        }

        var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
        if (tenant is null)
        {
            throw new NotFoundException("مستاجر مورد نظر یافت نشد.");
        }

        return tenant.Branches
            .OrderBy(branch => branch.Name)
            .Select(branch => new BranchSummaryDto(
                branch.Id.Value,
                branch.Name,
                branch.Address.City,
                branch.Address.Street,
                branch.Address.PostalCode,
                branch.Tables.Count,
                branch.WorkingHours.Count))
            .ToList();
    }
}
