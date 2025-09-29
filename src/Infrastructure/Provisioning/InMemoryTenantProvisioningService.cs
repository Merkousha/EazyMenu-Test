using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Common.Interfaces.Provisioning;
using EazyMenu.Domain.Entities;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Infrastructure.Provisioning;

/// <summary>
/// Minimal in-memory implementation to unblock early development.
/// </summary>
public sealed class InMemoryTenantProvisioningService : ITenantProvisioningService
{
    private readonly ConcurrentDictionary<TenantId, Restaurant> _tenants = new();

    public Task<TenantId> ProvisionAsync(Restaurant restaurant, CancellationToken cancellationToken = default)
    {
        var tenantId = restaurant.TenantId.Value != Guid.Empty
            ? restaurant.TenantId
            : TenantId.New();

        _tenants.AddOrUpdate(tenantId, restaurant, (_, _) => restaurant);
        return Task.FromResult(tenantId);
    }
}
