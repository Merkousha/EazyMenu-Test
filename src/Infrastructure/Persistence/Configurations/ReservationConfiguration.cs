using System;
using EazyMenu.Domain.Aggregates.Reservations;
using EazyMenu.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EazyMenu.Infrastructure.Persistence.Configurations;

internal sealed class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("Reservations");

        builder.HasKey(reservation => reservation.Id);

        builder.Property(reservation => reservation.Id)
            .HasConversion(id => id.Value, value => ToReservationId(value))
            .ValueGeneratedNever();

        builder.Property(reservation => reservation.TenantId)
            .HasConversion(id => id.Value, value => ToTenantId(value))
            .HasColumnName("TenantId")
            .IsRequired();

        builder.Property(reservation => reservation.BranchId)
            .HasConversion(id => id.Value, value => ToBranchId(value))
            .HasColumnName("BranchId")
            .IsRequired();

        builder.Property(reservation => reservation.TableId)
            .HasConversion(id => id.Value, value => ToTableId(value))
            .HasColumnName("TableId")
            .IsRequired();

        builder.OwnsOne(reservation => reservation.ScheduleSlot, slot =>
        {
            slot.Property(s => s.DayOfWeek)
                .HasColumnName("ScheduleDayOfWeek")
                .HasConversion<int>()
                .IsRequired();

            slot.Property(s => s.Start)
                .HasColumnName("ScheduleStart")
                .HasColumnType("time")
                .IsRequired();

            slot.Property(s => s.End)
                .HasColumnName("ScheduleEnd")
                .HasColumnType("time")
                .IsRequired();
        });

        builder.Property(reservation => reservation.PartySize)
            .IsRequired();

        builder.Property(reservation => reservation.SpecialRequest)
            .HasMaxLength(1000);

        builder.Property(reservation => reservation.CustomerName)
            .HasMaxLength(200);

        builder.Property(reservation => reservation.CustomerPhone)
            .HasConversion(phone => phone != null ? phone.Value : null, value => value != null ? PhoneNumber.Create(value) : null)
            .HasColumnName("CustomerPhone")
            .HasMaxLength(32);

        builder.Property(reservation => reservation.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(reservation => reservation.CreatedAtUtc)
            .IsRequired();

        builder.Property(reservation => reservation.ConfirmedAtUtc);
        builder.Property(reservation => reservation.CancelledAtUtc);
        builder.Property(reservation => reservation.CheckedInAtUtc);
        builder.Property(reservation => reservation.NoShowRecordedAtUtc);

        var historyNavigation = builder.OwnsMany(reservation => reservation.StatusHistory, history =>
        {
            history.ToTable("ReservationStatusHistory");
            history.WithOwner().HasForeignKey("ReservationId");

            history.Property<Guid>("Id").ValueGeneratedOnAdd();
            history.HasKey("Id");

            history.Property(entry => entry.Status)
                .HasConversion<int>()
                .IsRequired();

            history.Property(entry => entry.ChangedAtUtc)
                .IsRequired();

            history.Property(entry => entry.Note)
                .HasMaxLength(1000);
        });

        // Configure backing field for the StatusHistory collection
        builder.Navigation(nameof(Reservation.StatusHistory))
            .HasField("_statusHistory")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(reservation => reservation.DomainEvents);

    builder.HasIndex(reservation => new { reservation.TenantId, reservation.BranchId, reservation.TableId, reservation.Status });
    }

    private static ReservationId ToReservationId(Guid value)
    {
        if (!ReservationId.TryCreate(value, out var reservationId))
        {
            throw new InvalidOperationException("شناسه رزرو ذخیره‌شده نامعتبر است.");
        }

        return reservationId;
    }

    private static TenantId ToTenantId(Guid value)
    {
        if (!TenantId.TryCreate(value, out var tenantId))
        {
            throw new InvalidOperationException("شناسه مستاجر ذخیره‌شده نامعتبر است.");
        }

        return tenantId;
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
}
