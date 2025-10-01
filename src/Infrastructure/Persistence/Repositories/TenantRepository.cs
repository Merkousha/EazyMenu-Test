using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Domain.Aggregates.Tenants;
using EazyMenu.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EazyMenu.Infrastructure.Persistence.Repositories;

internal sealed class TenantRepository : ITenantRepository
{
    private readonly EazyMenuDbContext _dbContext;

    public TenantRepository(EazyMenuDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Tenant?> GetByIdAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await _dbContext.Tenants
            .AsSplitQuery()
            .Include(t => t.Branches)
                .ThenInclude(b => b.Tables)
            .Include(t => t.Branches)
                .ThenInclude(b => b.WorkingHours)
            .Include(t => t.Branches)
                .ThenInclude(b => b.QrCodes)
            .Include(t => t.Subscriptions)
            .Include(t => t.ActiveSubscription)
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);

        return tenant;
    }

    public async Task<Tenant?> GetBySlugAsync(TenantSlug slug, CancellationToken cancellationToken = default)
    {
        var tenant = await _dbContext.Tenants
            .AsSplitQuery()
            .Include(t => t.Branches)
                .ThenInclude(b => b.Tables)
            .Include(t => t.Branches)
                .ThenInclude(b => b.WorkingHours)
            .Include(t => t.Branches)
                .ThenInclude(b => b.QrCodes)
            .Include(t => t.Subscriptions)
            .Include(t => t.ActiveSubscription)
            .FirstOrDefaultAsync(t => t.Slug == slug, cancellationToken);

        return tenant;
    }

    public async Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        await _dbContext.Tenants.AddAsync(tenant, cancellationToken);
    }

    public Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        _dbContext.Tenants.Update(tenant);
        return Task.CompletedTask;
    }
}
