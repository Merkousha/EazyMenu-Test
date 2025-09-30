using System;
using EazyMenu.Domain.Aggregates.Tenants;
using EazyMenu.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EazyMenu.Infrastructure.Persistence.Configurations;

internal sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("TenantSubscriptions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedNever()
            .HasColumnName("SubscriptionId");

        builder.Property<TenantId>("TenantId")
            .HasColumnName("TenantId")
            .HasConversion(id => id.Value, value => ToTenantId(value))
            .IsRequired();

        builder.HasOne<Tenant>()
            .WithMany("_subscriptions")
            .HasForeignKey("TenantId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(s => s.Plan)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(s => s.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(s => s.StartDateUtc)
            .IsRequired();

        builder.Property(s => s.EndDateUtc);

        builder.Property(s => s.IsTrial)
            .IsRequired();

        builder.OwnsOne(s => s.Price, price =>
        {
            price.Property(p => p.Amount)
                .HasColumnName("PriceAmount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            price.Property(p => p.Currency)
                .HasColumnName("PriceCurrency")
                .HasMaxLength(8)
                .IsRequired();
        });

        builder.OwnsOne(s => s.DiscountPercentage, discount =>
        {
            discount.Property(d => d.Value)
                .HasColumnName("DiscountPercentage")
                .HasColumnType("decimal(5,2)");
        });

        builder.Property(s => s.DiscountCode)
            .HasMaxLength(128);

        builder.Ignore(s => s.DomainEvents);
    }

    private static TenantId ToTenantId(Guid value)
    {
        if (!TenantId.TryCreate(value, out var tenantId))
        {
            throw new InvalidOperationException("شناسه مستاجر ذخیره‌شده نامعتبر است.");
        }

        return tenantId;
    }
}
