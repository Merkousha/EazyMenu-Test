using System.Collections.Generic;
using EazyMenu.Domain.Aggregates.Tenants;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Domain.Services.Scheduling;

public interface IReservationSchedulingPolicy
{
    TableAllocationResult Allocate(
        IReadOnlyCollection<Table> tables,
        IReadOnlyCollection<ScheduledReservation> existingReservations,
        ScheduleSlot desiredSlot,
        int partySize,
        bool prefersOutdoor = false);
}
