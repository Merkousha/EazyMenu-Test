using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System;
using EazyMenu.Application.Common.Interfaces.Provisioning;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Infrastructure.Provisioning;

/// <summary>
/// Minimal in-memory implementation to unblock early development.
/// </summary>
public sealed class InMemoryTenantProvisioningService : ITenantProvisioningService
{
    private readonly ConcurrentDictionary<TenantId, TenantProvisioningSnapshot> _tenants = new();

    public Task<TenantProvisioningResult> ProvisionAsync(TenantProvisioningRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = TenantId.New();
        var subscriptionId = Guid.NewGuid();

        var snapshot = new TenantProvisioningSnapshot(
            request.RestaurantName,
            request.ManagerEmail,
            request.ManagerPhone,
            request.PlanCode,
            request.HeadquartersAddress,
            request.UseTrial,
            request.DiscountCode);

        _tenants.AddOrUpdate(tenantId, snapshot, (_, _) => snapshot);
        var result = new TenantProvisioningResult(tenantId, subscriptionId, null);
        return Task.FromResult(result);
    }

    private sealed record TenantProvisioningSnapshot(
        string RestaurantName,
        string ManagerEmail,
        string ManagerPhone,
        string PlanCode,
        Address HeadquartersAddress,
        bool UseTrial,
        string? DiscountCode);
}
