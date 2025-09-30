using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EazyMenu.Web.Services;

public sealed class DashboardTenantProvider : IDashboardTenantProvider
{
    private readonly EazyMenuDbContext _dbContext;
    private Guid? _cachedTenantId;

    public DashboardTenantProvider(EazyMenuDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid?> GetActiveTenantIdAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedTenantId.HasValue)
        {
            return _cachedTenantId.Value;
        }

        var tenantId = await _dbContext.Menus
            .AsNoTracking()
            .OrderByDescending(menu => menu.IsDefault)
            .ThenBy(menu => menu.CreatedAtUtc)
            .Select(menu => menu.TenantId.Value)
            .FirstOrDefaultAsync(cancellationToken);

        if (tenantId == Guid.Empty)
        {
            tenantId = await _dbContext.Tenants
                .AsNoTracking()
                .OrderBy(tenant => tenant.CreatedAtUtc)
                .Select(tenant => tenant.Id.Value)
                .FirstOrDefaultAsync(cancellationToken);

            if (tenantId == Guid.Empty)
            {
                return null;
            }
        }

        _cachedTenantId = tenantId;
        return tenantId;
    }
}
