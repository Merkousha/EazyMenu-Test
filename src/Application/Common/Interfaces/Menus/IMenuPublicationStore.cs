using System;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Features.Menus.Common;

namespace EazyMenu.Application.Common.Interfaces.Menus;

public interface IMenuPublicationWriter
{
    Task SaveAsync(PublishedMenuDto snapshot, CancellationToken cancellationToken = default);
}

public interface IMenuPublicationReader
{
    Task<PublishedMenuDto?> GetLatestAsync(Guid tenantId, Guid? menuId, CancellationToken cancellationToken = default);
    Task<PublishedMenuDto?> GetVersionAsync(Guid tenantId, Guid menuId, int version, CancellationToken cancellationToken = default);
}
