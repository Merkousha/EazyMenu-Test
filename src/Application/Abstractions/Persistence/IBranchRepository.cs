using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Domain.Aggregates.Tenants;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Abstractions.Persistence;

public interface IBranchRepository
{
    Task<Branch?> GetByIdAsync(TenantId tenantId, BranchId branchId, CancellationToken cancellationToken = default);

    Task UpdateAsync(Branch branch, CancellationToken cancellationToken = default);
}
