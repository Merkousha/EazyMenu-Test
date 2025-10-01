using System;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Common.Interfaces.Menus;
using EazyMenu.Application.Features.Menus.Common;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Menus.Queries.GetPublishedMenu;

public sealed class GetPublishedMenuQueryHandler : IQueryHandler<GetPublishedMenuQuery, PublishedMenuDto?>
{
    private readonly IMenuPublicationReader _publicationReader;

    public GetPublishedMenuQueryHandler(IMenuPublicationReader publicationReader)
    {
        _publicationReader = publicationReader;
    }

    public async Task<PublishedMenuDto?> HandleAsync(GetPublishedMenuQuery query, CancellationToken cancellationToken = default)
    {
        if (!TenantId.TryCreate(query.TenantId, out var tenantId))
        {
            throw new BusinessRuleViolationException("شناسه مستاجر معتبر نیست.");
        }

        Guid? menuId = null;
        if (query.MenuId is not null)
        {
            if (!MenuId.TryCreate(query.MenuId.Value, out var mid))
            {
                throw new BusinessRuleViolationException("شناسه منو معتبر نیست.");
            }

            menuId = mid.Value;
        }

        return await _publicationReader.GetLatestAsync(tenantId.Value, menuId, cancellationToken);
    }
}
