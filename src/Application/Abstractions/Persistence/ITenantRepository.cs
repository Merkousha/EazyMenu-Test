using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Domain.Aggregates.Tenants;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Abstractions.Persistence;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(TenantId tenantId, CancellationToken cancellationToken = default);

    Task<Tenant?> GetBySlugAsync(TenantSlug slug, CancellationToken cancellationToken = default);

    Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default);

    Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default);
}
