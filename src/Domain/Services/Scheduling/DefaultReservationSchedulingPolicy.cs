using System;
using System.Collections.Generic;
using System.Linq;
using EazyMenu.Domain.Aggregates.Reservations;
using EazyMenu.Domain.Aggregates.Tenants;
using EazyMenu.Domain.Common.Guards;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Domain.Services.Scheduling;

public sealed class DefaultReservationSchedulingPolicy : IReservationSchedulingPolicy
{
    public TableAllocationResult Allocate(
        IReadOnlyCollection<Table> tables,
        IReadOnlyCollection<ScheduledReservation> existingReservations,
        ScheduleSlot desiredSlot,
        int partySize,
        bool prefersOutdoor = false)
    {
        Guard.AgainstNull(tables, nameof(tables));
        Guard.AgainstNull(existingReservations, nameof(existingReservations));
        Guard.AgainstNull(desiredSlot, nameof(desiredSlot));

        if (tables.Count == 0)
        {
            return TableAllocationResult.Failure("برای این شعبه میزی تعریف نشده است.");
        }

        var candidates = tables
            .Where(table => !table.IsOutOfService && table.Capacity >= partySize)
            .OrderBy(table => table.Capacity)
            .ThenBy(table => table.Label, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (prefersOutdoor)
        {
            candidates = candidates
                .OrderByDescending(table => table.IsOutdoor)
                .ThenBy(table => table.Capacity)
                .ThenBy(table => table.Label, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        foreach (var table in candidates)
        {
            var hasConflict = existingReservations.Any(reservation =>
                reservation.TableId == table.Id &&
                IsBlockingStatus(reservation.Status) &&
                reservation.ScheduleSlot.Overlaps(desiredSlot));

            if (!hasConflict)
            {
                return TableAllocationResult.Success(table.Id);
            }
        }

        return TableAllocationResult.Failure("ظرفیت خالی مطابق بازه زمانی موردنظر یافت نشد.");
    }

    private static bool IsBlockingStatus(ReservationStatus status)
    {
        return status is ReservationStatus.Pending or ReservationStatus.Confirmed or ReservationStatus.CheckedIn;
    }
}
