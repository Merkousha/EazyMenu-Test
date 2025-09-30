using System;
using EazyMenu.Domain.Aggregates.Tenants;
using EazyMenu.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EazyMenu.Infrastructure.Persistence.Configurations;

internal sealed class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("TenantBranches");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .HasConversion(id => id.Value, value => ToBranchId(value))
            .ValueGeneratedNever()
            .HasColumnName("BranchId");

        builder.Property<TenantId>("TenantId")
            .HasColumnName("TenantId")
            .HasConversion(id => id.Value, value => ToTenantId(value))
            .IsRequired();

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey("TenantId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(b => b.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.OwnsOne(b => b.Address, address =>
        {
            address.Property(a => a.City)
                .HasColumnName("City")
                .HasMaxLength(128)
                .IsRequired();

            address.Property(a => a.Street)
                .HasColumnName("Street")
                .HasMaxLength(256)
                .IsRequired();

            address.Property(a => a.PostalCode)
                .HasColumnName("PostalCode")
                .HasMaxLength(32)
                .IsRequired();
        });

        builder.OwnsMany<ScheduleSlot>(b => b.WorkingHours, hours =>
        {
            hours.ToTable("BranchWorkingHours");
            hours.WithOwner().HasForeignKey("BranchId");

            hours.Property<BranchId>("BranchId")
                .HasColumnName("BranchId")
                .HasConversion(id => id.Value, value => ToBranchId(value))
                .IsRequired();

            hours.Property<Guid>("Id").ValueGeneratedOnAdd();
            hours.HasKey("Id");

            hours.Property(h => h.DayOfWeek)
                .HasConversion<int>()
                .IsRequired();

            hours.Property(h => h.Start)
                .HasColumnType("time")
                .IsRequired();

            hours.Property(h => h.End)
                .HasColumnType("time")
                .IsRequired();
        });
        builder.Navigation(b => b.WorkingHours)
            .HasField("_workingHours")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany<Table>(b => b.Tables, table =>
        {
            table.ToTable("BranchTables");
            table.WithOwner().HasForeignKey("BranchId");

            table.Property<BranchId>("BranchId")
                .HasColumnName("BranchId")
                .HasConversion(id => id.Value, value => ToBranchId(value))
                .IsRequired();

            table.HasKey(t => t.Id);

            table.Property(t => t.Id)
                .HasConversion(id => id.Value, value => ToTableId(value))
                .ValueGeneratedNever()
                .HasColumnName("TableId");

            table.Property(t => t.Label)
                .HasMaxLength(100)
                .IsRequired();

            table.Property(t => t.Capacity).IsRequired();
            table.Property(t => t.IsOutdoor).IsRequired();
            table.Property(t => t.IsOutOfService).IsRequired();

            table.Ignore(t => t.DomainEvents);
        });
        builder.Navigation(b => b.Tables)
            .HasField("_tables")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany<QrCodeReference>(b => b.QrCodes, qr =>
        {
            qr.ToTable("BranchQrCodes");
            qr.WithOwner().HasForeignKey("BranchId");

            qr.Property<BranchId>("BranchId")
                .HasColumnName("BranchId")
                .HasConversion(id => id.Value, value => ToBranchId(value))
                .IsRequired();

            qr.Property<Guid>("Id").ValueGeneratedOnAdd();
            qr.HasKey("Id");

            qr.Property(q => q.Value)
                .HasColumnName("Code")
                .HasMaxLength(128)
                .IsRequired();
        });
        builder.Navigation(b => b.QrCodes)
            .HasField("_qrCodes")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(b => b.DomainEvents);
    }

    private static BranchId ToBranchId(Guid value)
    {
        if (!BranchId.TryCreate(value, out var branchId))
        {
            throw new InvalidOperationException("شناسه شعبه ذخیره‌شده نامعتبر است.");
        }

        return branchId;
    }

    private static TableId ToTableId(Guid value)
    {
        if (!TableId.TryCreate(value, out var tableId))
        {
            throw new InvalidOperationException("شناسه میز ذخیره‌شده نامعتبر است.");
        }

        return tableId;
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
