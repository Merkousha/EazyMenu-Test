using EazyMenu.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EazyMenu.Infrastructure.Persistence.Configurations;

internal sealed class SmsDeliveryLogConfiguration : IEntityTypeConfiguration<SmsDeliveryLog>
{
    public void Configure(EntityTypeBuilder<SmsDeliveryLog> builder)
    {
        builder.ToTable("SmsDeliveryLogs");

        builder.HasKey(log => log.Id);

        builder.Property(log => log.PhoneNumber)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(log => log.Message)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(log => log.Provider)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(log => log.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(log => log.OccurredAt)
            .IsRequired();

        builder.Property(log => log.ErrorCode)
            .HasMaxLength(64);

        builder.Property(log => log.ErrorMessage)
            .HasMaxLength(512);

        builder.Property(log => log.Payload)
            .HasMaxLength(2048);

        builder.Property(log => log.TenantId)
            .IsRequired(false);

        builder.Property(log => log.SubscriptionPlan)
            .HasConversion<int?>()
            .IsRequired(false);

        builder.HasIndex(log => log.PhoneNumber);
        builder.HasIndex(log => log.Status);
        builder.HasIndex(log => log.OccurredAt);
        builder.HasIndex(log => log.TenantId);
    }
}
