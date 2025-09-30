using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Domain.Aggregates.Menus;
using EazyMenu.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EazyMenu.Infrastructure.Persistence.Repositories;

internal sealed class MenuRepository : IMenuRepository
{
    private readonly EazyMenuDbContext _dbContext;

    public MenuRepository(EazyMenuDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Menu?> GetByIdAsync(TenantId tenantId, MenuId menuId, CancellationToken cancellationToken = default)
    {
        var menu = await _dbContext.Menus
            .AsSplitQuery()
            .Include(m => m.Categories)
            .ThenInclude(c => c.Items)
            .FirstOrDefaultAsync(m => m.Id == menuId && m.TenantId == tenantId, cancellationToken);

        return menu;
    }

    public async Task<IReadOnlyCollection<Menu>> GetByTenantAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var menus = await _dbContext.Menus
            .AsSplitQuery()
            .Include(m => m.Categories)
            .ThenInclude(c => c.Items)
            .Where(m => m.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        return menus;
    }

    public async Task AddAsync(Menu menu, CancellationToken cancellationToken = default)
    {
        await _dbContext.Menus.AddAsync(menu, cancellationToken);
    }

    public Task UpdateAsync(Menu menu, CancellationToken cancellationToken = default)
    {
        _dbContext.Menus.Update(menu);
        return Task.CompletedTask;
    }
}
