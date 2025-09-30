using EazyMenu.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EazyMenu.Infrastructure.Persistence.Configurations;

internal sealed class TenantProvisioningRecordConfiguration : IEntityTypeConfiguration<TenantProvisioningRecord>
{
    public void Configure(EntityTypeBuilder<TenantProvisioningRecord> builder)
    {
        builder.ToTable("TenantProvisionings");

        builder.HasKey(record => record.TenantId);

        builder.Property(record => record.RestaurantName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(record => record.RestaurantSlug)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(record => record.ManagerEmail)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(record => record.ManagerPhone)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(record => record.PlanCode)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(record => record.City)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(record => record.Street)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(record => record.PostalCode)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(record => record.DiscountCode)
            .HasMaxLength(64);

        builder.Property(record => record.CreatedAtUtc)
            .IsRequired();
    }
}
