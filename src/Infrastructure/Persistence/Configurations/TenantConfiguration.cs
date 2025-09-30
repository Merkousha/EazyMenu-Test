using System;
using EazyMenu.Domain.Aggregates.Tenants;
using EazyMenu.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EazyMenu.Infrastructure.Persistence.Configurations;

internal sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasConversion(id => id.Value, value => ToTenantId(value))
            .ValueGeneratedNever();

        builder.Property(t => t.BusinessName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.CreatedAtUtc).IsRequired();
        builder.Property(t => t.IsSuspended).IsRequired();
        builder.Property(t => t.SuspendedAtUtc);

        builder.Property(t => t.ContactEmail)
            .HasConversion(email => email.Value, value => Email.Create(value))
            .HasColumnName("ContactEmail")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(t => t.ContactPhone)
            .HasConversion(phone => phone.Value, value => PhoneNumber.Create(value))
            .HasColumnName("ContactPhone")
            .HasMaxLength(32)
            .IsRequired();

        builder.OwnsOne(t => t.Branding, brand =>
        {
            brand.Property(b => b.DisplayName)
                .HasColumnName("BrandDisplayName")
                .HasMaxLength(200)
                .IsRequired();

            brand.Property(b => b.LogoUrl)
                .HasColumnName("BrandLogoUrl")
                .HasMaxLength(512);

            brand.Property(b => b.PrimaryColor)
                .HasColumnName("BrandPrimaryColor")
                .HasMaxLength(32);
        });

        builder.OwnsOne(t => t.HeadquartersAddress, address =>
        {
            address.Property(a => a.City)
                .HasColumnName("HeadquartersCity")
                .HasMaxLength(128);

            address.Property(a => a.Street)
                .HasColumnName("HeadquartersStreet")
                .HasMaxLength(256);

            address.Property(a => a.PostalCode)
                .HasColumnName("HeadquartersPostalCode")
                .HasMaxLength(32);
        });
        builder.Navigation(t => t.HeadquartersAddress).IsRequired(false);

        builder.Ignore(t => t.DomainEvents);

        builder.Property<Guid?>("ActiveSubscriptionId")
            .HasColumnName("ActiveSubscriptionId");

        builder.HasMany(t => t.Branches)
            .WithOne()
            .HasForeignKey("TenantId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Subscriptions)
            .WithOne()
            .HasForeignKey("TenantId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.ActiveSubscription)
            .WithMany()
            .HasForeignKey("ActiveSubscriptionId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(t => t.Branches)
            .HasField("_branches")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(t => t.Subscriptions)
            .HasField("_subscriptions")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(t => t.ActiveSubscription)
            .HasField("_activeSubscription")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
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
