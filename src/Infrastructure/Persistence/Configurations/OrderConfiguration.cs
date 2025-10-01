using System;
using EazyMenu.Domain.Aggregates.Orders;
using EazyMenu.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EazyMenu.Infrastructure.Persistence.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasConversion(
                orderId => orderId.Value,
                value => OrderId.FromGuid(value))
            .HasColumnName("OrderId");

        builder.Property(o => o.TenantId)
            .HasConversion(
                tenantId => tenantId.Value,
                value => TenantId.FromGuid(value))
            .HasColumnName("TenantId")
            .IsRequired();

        builder.Property(o => o.MenuId)
            .HasColumnName("MenuId")
            .IsRequired();

        builder.Property(o => o.OrderNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(o => o.FulfillmentMethod)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(o => o.CustomerName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(o => o.CustomerPhone)
            .HasConversion(
                phone => phone.Value,
                value => PhoneNumber.Create(value))
            .HasColumnName("CustomerPhone")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(o => o.CustomerNote)
            .HasMaxLength(1000);

        builder.Property(o => o.DeliveryFee)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(o => o.TaxAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(o => o.SubtotalAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(o => o.CreatedAtUtc)
            .IsRequired();

        builder.Property(o => o.ConfirmedAtUtc);

        builder.Property(o => o.CompletedAtUtc);

        builder.Property(o => o.CancelledAtUtc);

        builder.Property(o => o.CancellationReason)
            .HasMaxLength(500);

        // Configure OrderItems as owned entities
        builder.OwnsMany(o => o.Items, itemBuilder =>
        {
            itemBuilder.ToTable("OrderItems");

            itemBuilder.WithOwner().HasForeignKey("OrderId");

            itemBuilder.HasKey("Id");

            itemBuilder.Property(i => i.Id)
                .HasConversion(
                    id => id.Value,
                    value => OrderItemId.FromGuid(value))
                .HasColumnName("OrderItemId");

            itemBuilder.Property(i => i.MenuItemId)
                .HasColumnName("MenuItemId")
                .IsRequired();

            itemBuilder.Property(i => i.DisplayName)
                .HasMaxLength(200)
                .IsRequired();

            itemBuilder.Property(i => i.UnitPrice)
                .HasPrecision(18, 2)
                .IsRequired();

            itemBuilder.Property(i => i.Quantity)
                .IsRequired();

            itemBuilder.Property(i => i.Note)
                .HasMaxLength(500);

            itemBuilder.Ignore(i => i.TotalAmount);
        });

        builder.Ignore(o => o.TotalAmount);

        // Index for tenant-based queries
        builder.HasIndex(o => o.TenantId);

        // Index for order number uniqueness
        builder.HasIndex(o => new { o.TenantId, o.OrderNumber })
            .IsUnique();

        // Index for status filtering
        builder.HasIndex(o => new { o.TenantId, o.Status });

        // Index for date-based queries
        builder.HasIndex(o => o.CreatedAtUtc);
    }
}
