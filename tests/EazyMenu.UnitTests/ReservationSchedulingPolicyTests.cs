using System;
using System.Collections.Generic;
using EazyMenu.Domain.Aggregates.Reservations;
using EazyMenu.Domain.Aggregates.Tenants;
using EazyMenu.Domain.Services.Scheduling;
using EazyMenu.Domain.ValueObjects;
using Xunit;

namespace EazyMenu.UnitTests;

public class ReservationSchedulingPolicyTests
{
    private readonly DefaultReservationSchedulingPolicy _policy = new();

    [Fact]
    public void Allocate_WhenTableAvailable_ReturnsSuccess()
    {
        var tables = new List<Table>
        {
            Table.Create("A1", 4)
        };
        var existing = new List<ScheduledReservation>();
        var slot = ScheduleSlot.Create(DayOfWeek.Friday, new TimeSpan(18, 0, 0), new TimeSpan(20, 0, 0));

        var result = _policy.Allocate(tables, existing, slot, 2);

        Assert.True(result.IsSuccessful);
        Assert.Equal(tables[0].Id, result.TableId);
    }

    [Fact]
    public void Allocate_WhenAllTablesBusy_ReturnsFailure()
    {
        var table = Table.Create("A1", 4);
        var tables = new List<Table> { table };
        var slot = ScheduleSlot.Create(DayOfWeek.Friday, new TimeSpan(18, 0, 0), new TimeSpan(20, 0, 0));
        var existing = new List<ScheduledReservation>
        {
            new ScheduledReservation(ReservationId.New(), table.Id, slot, ReservationStatus.Confirmed)
        };

        var result = _policy.Allocate(tables, existing, slot, 2);

        Assert.False(result.IsSuccessful);
        Assert.Null(result.TableId);
        Assert.NotNull(result.FailureReason);
    }

    [Fact]
    public void Allocate_WhenPrefersOutdoor_PrioritizesOutdoorTable()
    {
        var indoor = Table.Create("A1", 4);
        var outdoor = Table.Create("Garden", 4);
        outdoor.SetOutdoor(true);
        var tables = new List<Table> { indoor, outdoor };
        var existing = new List<ScheduledReservation>();
        var slot = ScheduleSlot.Create(DayOfWeek.Saturday, new TimeSpan(12, 0, 0), new TimeSpan(14, 0, 0));

        var result = _policy.Allocate(tables, existing, slot, 2, prefersOutdoor: true);

        Assert.True(result.IsSuccessful);
        Assert.Equal(outdoor.Id, result.TableId);
    }
}
