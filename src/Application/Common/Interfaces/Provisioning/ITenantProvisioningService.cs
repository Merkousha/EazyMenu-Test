using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Common.Interfaces.Provisioning;

public interface ITenantProvisioningService
{
    Task<TenantProvisioningResult> ProvisionAsync(TenantProvisioningRequest request, CancellationToken cancellationToken = default);
}
