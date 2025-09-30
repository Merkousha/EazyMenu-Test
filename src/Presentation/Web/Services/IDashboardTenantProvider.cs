using System;
using System.Threading;
using System.Threading.Tasks;

namespace EazyMenu.Web.Services;

public interface IDashboardTenantProvider
{
    Task<Guid?> GetActiveTenantIdAsync(CancellationToken cancellationToken = default);
}
