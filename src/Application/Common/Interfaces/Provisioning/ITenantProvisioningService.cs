using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Domain.Entities;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Common.Interfaces.Provisioning;

public interface ITenantProvisioningService
{
    Task<TenantId> ProvisionAsync(Restaurant restaurant, CancellationToken cancellationToken = default);
}
