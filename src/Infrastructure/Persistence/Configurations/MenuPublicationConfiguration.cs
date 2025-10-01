using EazyMenu.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EazyMenu.Infrastructure.Persistence.Configurations;

public sealed class MenuPublicationConfiguration : IEntityTypeConfiguration<MenuPublication>
{
    public void Configure(EntityTypeBuilder<MenuPublication> builder)
    {
        builder.ToTable("MenuPublications");

        builder.HasKey(publication => publication.Id);

        builder.Property(publication => publication.SnapshotJson)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(publication => publication.PublishedAtUtc)
            .IsRequired();

        builder.Property(publication => publication.Version)
            .IsRequired();

        builder.Property(publication => publication.MenuId)
            .IsRequired();

        builder.Property(publication => publication.TenantId)
            .IsRequired();

        builder.HasIndex(publication => new { publication.TenantId, publication.MenuId, publication.Version })
            .IsUnique();

        builder.HasIndex(publication => new { publication.TenantId, publication.PublishedAtUtc })
            .HasDatabaseName("IX_MenuPublications_Tenant_PublishedAt");
    }
}
