using System;
using EazyMenu.Domain.Aggregates.Payments;
using EazyMenu.Domain.Aggregates.Tenants;
using EazyMenu.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EazyMenu.Infrastructure.Persistence.Configurations;

internal sealed class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.ToTable("PaymentTransactions");

        builder.HasKey(transaction => transaction.Id);

        builder.Property(transaction => transaction.Id)
            .ValueGeneratedNever()
            .HasConversion(id => id.Value, value => ToPaymentId(value))
            .HasColumnName("PaymentId");

        builder.Property(transaction => transaction.TenantId)
            .HasConversion(id => id.Value, value => ToTenantId(value))
            .IsRequired();

        builder.HasIndex(transaction => transaction.TenantId);
        builder.HasIndex(transaction => transaction.SubscriptionId);

        builder.Property(transaction => transaction.SubscriptionId);

        builder.Property(transaction => transaction.Method)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(transaction => transaction.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(transaction => transaction.Description)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(transaction => transaction.DiscountCode)
            .HasMaxLength(64);

        builder.Property(transaction => transaction.IssuedAtUtc)
            .IsRequired();

        builder.Property(transaction => transaction.CompletedAtUtc);
        builder.Property(transaction => transaction.RefundedAtUtc);

        builder.Property(transaction => transaction.ExternalReference)
            .HasMaxLength(128);

        builder.Property(transaction => transaction.FailureReason)
            .HasMaxLength(512);

        builder.Property(transaction => transaction.GatewayAuthority)
            .HasMaxLength(128);

        builder.OwnsOne(transaction => transaction.Amount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("NetAmount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(8)
                .IsRequired();
        });

        builder.OwnsOne(transaction => transaction.OriginalAmount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("OriginalAmount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("OriginalCurrency")
                .HasMaxLength(8)
                .IsRequired();
        });

        builder.OwnsOne(transaction => transaction.DiscountPercentage, discount =>
        {
            discount.Property(p => p.Value)
                .HasColumnName("DiscountPercentage")
                .HasColumnType("decimal(5,2)");
        });

        builder.Ignore(transaction => transaction.DiscountAmount);
        builder.Ignore(transaction => transaction.HasDiscount);
        builder.Ignore(transaction => transaction.DomainEvents);
    }

    private static PaymentId ToPaymentId(Guid value)
    {
        if (!PaymentId.TryCreate(value, out var paymentId))
        {
            throw new InvalidOperationException("شناسه پرداخت ذخیره‌شده نامعتبر است.");
        }

        return paymentId;
    }

    private static TenantId ToTenantId(Guid value)
    {
        if (!TenantId.TryCreate(value, out var tenantId))
        {
            throw new InvalidOperationException("شناسه مستاجر برای تراکنش پرداخت نامعتبر است.");
        }

        return tenantId;
    }
}
