using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Features.Menus.Common;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Menus.Queries.GetMenus;

public sealed class GetMenusQueryHandler : IQueryHandler<GetMenusQuery, IReadOnlyCollection<MenuSummaryDto>>
{
    private readonly IMenuRepository _menuRepository;

    public GetMenusQueryHandler(IMenuRepository menuRepository)
    {
        _menuRepository = menuRepository;
    }

    public async Task<IReadOnlyCollection<MenuSummaryDto>> HandleAsync(GetMenusQuery query, CancellationToken cancellationToken = default)
    {
        if (!TenantId.TryCreate(query.TenantId, out var tenantId))
        {
            throw new BusinessRuleViolationException("شناسه مستاجر معتبر نیست.");
        }

        var menus = await _menuRepository.GetByTenantAsync(tenantId, cancellationToken);
        var filtered = query.IncludeArchived
            ? menus
            : menus.Where(menu => !menu.IsArchived);

        return filtered
            .OrderByDescending(menu => menu.UpdatedAtUtc)
            .Select(MenuMapper.ToSummaryDto)
            .ToList();
    }
}
