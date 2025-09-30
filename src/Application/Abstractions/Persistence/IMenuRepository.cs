using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Domain.Aggregates.Menus;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Abstractions.Persistence;

public interface IMenuRepository
{
    Task<Menu?> GetByIdAsync(TenantId tenantId, MenuId menuId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Menu>> GetByTenantAsync(TenantId tenantId, CancellationToken cancellationToken = default);

    Task AddAsync(Menu menu, CancellationToken cancellationToken = default);

    Task UpdateAsync(Menu menu, CancellationToken cancellationToken = default);
}
