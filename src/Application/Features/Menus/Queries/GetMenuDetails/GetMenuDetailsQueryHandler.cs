using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Features.Menus.Common;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Menus.Queries.GetMenuDetails;

public sealed class GetMenuDetailsQueryHandler : IQueryHandler<GetMenuDetailsQuery, MenuDetailsDto>
{
    private readonly IMenuRepository _menuRepository;

    public GetMenuDetailsQueryHandler(IMenuRepository menuRepository)
    {
        _menuRepository = menuRepository;
    }

    public async Task<MenuDetailsDto> HandleAsync(GetMenuDetailsQuery query, CancellationToken cancellationToken = default)
    {
        if (!TenantId.TryCreate(query.TenantId, out var tenantId))
        {
            throw new BusinessRuleViolationException("شناسه مستاجر معتبر نیست.");
        }

        if (!MenuId.TryCreate(query.MenuId, out var menuId))
        {
            throw new BusinessRuleViolationException("شناسه منو معتبر نیست.");
        }

        var menu = await _menuRepository.GetByIdAsync(tenantId, menuId, cancellationToken);
        if (menu is null)
        {
            throw new NotFoundException("منوی مورد نظر یافت نشد.");
        }

        return MenuMapper.ToDetailsDto(menu, query.IncludeArchivedCategories);
    }
}
