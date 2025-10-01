using System;

namespace EazyMenu.Infrastructure.Persistence.Models;

public sealed class MenuPublication
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid MenuId { get; set; }

    public int Version { get; set; }

    public DateTime PublishedAtUtc { get; set; }

    public string SnapshotJson { get; set; } = string.Empty;
}
