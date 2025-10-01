using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Domain.Aggregates.Tenants;
using EazyMenu.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EazyMenu.Infrastructure.Persistence.Repositories;

internal sealed class BranchRepository : IBranchRepository
{
    private readonly EazyMenuDbContext _dbContext;

    public BranchRepository(EazyMenuDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Branch?> GetByIdAsync(TenantId tenantId, BranchId branchId, CancellationToken cancellationToken = default)
    {
        // Assumes Branches are included in Tenants aggregate
        var branch = await _dbContext.Branches
            .Include(b => b.Tables)
            .Include(b => b.WorkingHours)
            .Include(b => b.QrCodes)
            .FirstOrDefaultAsync(b => b.Id == branchId && b.TenantId == tenantId, cancellationToken);
        return branch;
    }

    public Task UpdateAsync(Branch branch, CancellationToken cancellationToken = default)
    {
        _dbContext.Branches.Update(branch);
        return Task.CompletedTask;
    }
}
