using System;
using System.Linq;
using EazyMenu.Domain.Aggregates.Reservations;
using EazyMenu.Domain.Common.Exceptions;
using EazyMenu.Domain.Events;
using EazyMenu.Domain.ValueObjects;
using Xunit;

namespace EazyMenu.UnitTests;

public class ReservationTests
{
    [Fact]
    public void Schedule_ShouldCreatePendingReservationAndRaiseEvent()
    {
        var tenantId = TenantId.New();
        var branchId = BranchId.New();
        var tableId = TableId.New();
        var slot = ScheduleSlot.Create(DayOfWeek.Friday, new TimeSpan(18, 0, 0), new TimeSpan(20, 0, 0));

        var reservation = Reservation.Schedule(tenantId, branchId, tableId, slot, 2, 4, customerName: "مشتری");

        Assert.Equal(ReservationStatus.Pending, reservation.Status);
        var historyEntry = reservation.StatusHistory.Single();
        Assert.Equal(ReservationStatus.Pending, historyEntry.Status);
        Assert.Contains(reservation.DomainEvents, e => e is ReservationCreatedDomainEvent);
    }

    [Fact]
    public void Confirm_ShouldChangeStatusAndRaiseEvent()
    {
        var reservation = CreateReservation();
        var confirmTime = DateTime.UtcNow;

        reservation.Confirm(confirmTime, "تایید شد");

        Assert.Equal(ReservationStatus.Confirmed, reservation.Status);
        Assert.Equal(confirmTime, reservation.ConfirmedAtUtc);
        Assert.Contains(reservation.DomainEvents, e => e is ReservationConfirmedDomainEvent);
    }

    [Fact]
    public void Cancel_ShouldSetStatusCancelledAndRaiseEvent()
    {
        var reservation = CreateReservation();
        var cancelTime = DateTime.UtcNow;

        reservation.Cancel(cancelTime, "مشتری لغو کرد");

        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
        Assert.Equal(cancelTime, reservation.CancelledAtUtc);
        Assert.Contains(reservation.DomainEvents, e => e is ReservationCancelledDomainEvent);
    }

    [Fact]
    public void MarkAsNoShow_WhenNotConfirmed_ShouldThrow()
    {
        var reservation = CreateReservation();

        Assert.Throws<DomainException>(() => reservation.MarkAsNoShow(DateTime.UtcNow));
    }

    [Fact]
    public void MarkAsCheckedIn_AfterConfirm_ShouldSucceed()
    {
        var reservation = CreateReservation();
        reservation.Confirm(DateTime.UtcNow);
        var checkInTime = DateTime.UtcNow.AddMinutes(30);

        reservation.MarkAsCheckedIn(checkInTime);

        Assert.Equal(ReservationStatus.CheckedIn, reservation.Status);
        Assert.Equal(checkInTime, reservation.CheckedInAtUtc);
        Assert.Contains(reservation.DomainEvents, e => e is ReservationCheckedInDomainEvent);
    }

    [Fact]
    public void UpdatePartySize_WhenExceedsCapacity_ShouldThrow()
    {
        var reservation = CreateReservation(tableCapacity: 4, partySize: 2);

        Assert.Throws<DomainException>(() => reservation.UpdatePartySize(6, 5, DateTime.UtcNow));
    }

    [Fact]
    public void Schedule_WhenPartySizeExceedsCapacity_ShouldThrow()
    {
        var tenantId = TenantId.New();
        var branchId = BranchId.New();
        var tableId = TableId.New();
        var slot = ScheduleSlot.Create(DayOfWeek.Monday, new TimeSpan(12, 0, 0), new TimeSpan(13, 0, 0));

        Assert.Throws<DomainException>(() => Reservation.Schedule(tenantId, branchId, tableId, slot, 6, 4));
    }

    private static Reservation CreateReservation(int tableCapacity = 4, int partySize = 2)
    {
        var tenantId = TenantId.New();
        var branchId = BranchId.New();
        var tableId = TableId.New();
        var slot = ScheduleSlot.Create(DayOfWeek.Friday, new TimeSpan(18, 0, 0), new TimeSpan(20, 0, 0));

        return Reservation.Schedule(tenantId, branchId, tableId, slot, partySize, tableCapacity, customerName: "مشتری");
    }
}
