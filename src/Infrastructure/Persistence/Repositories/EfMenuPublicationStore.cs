using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Common.Interfaces.Menus;
using EazyMenu.Application.Features.Menus.Common;
using EazyMenu.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace EazyMenu.Infrastructure.Persistence.Repositories;

internal sealed class EfMenuPublicationStore : IMenuPublicationWriter, IMenuPublicationReader
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private readonly EazyMenuDbContext _dbContext;

    public EfMenuPublicationStore(EazyMenuDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SaveAsync(PublishedMenuDto snapshot, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.MenuPublications
            .Where(publication => publication.TenantId == snapshot.TenantId &&
                                   publication.MenuId == snapshot.MenuId &&
                                   publication.Version == snapshot.Version)
            .ToListAsync(cancellationToken);

        if (existing.Count > 0)
        {
            _dbContext.MenuPublications.RemoveRange(existing);
        }

        var entity = new MenuPublication
        {
            Id = Guid.NewGuid(),
            TenantId = snapshot.TenantId,
            MenuId = snapshot.MenuId,
            Version = snapshot.Version,
            PublishedAtUtc = snapshot.PublishedAtUtc,
            SnapshotJson = JsonSerializer.Serialize(snapshot, SerializerOptions)
        };

        await _dbContext.MenuPublications.AddAsync(entity, cancellationToken);
    }

    public async Task<PublishedMenuDto?> GetLatestAsync(Guid tenantId, Guid? menuId, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.MenuPublications
            .Where(publication => publication.TenantId == tenantId);

        if (menuId.HasValue)
        {
            query = query.Where(publication => publication.MenuId == menuId.Value);
        }

        var entity = await query
            .OrderByDescending(publication => publication.PublishedAtUtc)
            .ThenByDescending(publication => publication.Version)
            .FirstOrDefaultAsync(cancellationToken);

        return entity is null
            ? null
            : JsonSerializer.Deserialize<PublishedMenuDto>(entity.SnapshotJson, SerializerOptions);
    }

    public async Task<PublishedMenuDto?> GetVersionAsync(Guid tenantId, Guid menuId, int version, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.MenuPublications
            .Where(publication => publication.TenantId == tenantId &&
                                   publication.MenuId == menuId &&
                                   publication.Version == version)
            .OrderByDescending(publication => publication.PublishedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        return entity is null
            ? null
            : JsonSerializer.Deserialize<PublishedMenuDto>(entity.SnapshotJson, SerializerOptions);
    }
}
