using System;
using System.Threading;
using System.Threading.Tasks;

namespace EazyMenu.Web.Services;

public interface IDashboardTenantProvider
{
    Task<Guid?> GetActiveTenantIdAsync(CancellationToken cancellationToken = default);
    
    Task<Guid?> GetDefaultBranchIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
