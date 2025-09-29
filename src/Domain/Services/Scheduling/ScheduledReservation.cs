using EazyMenu.Domain.Aggregates.Reservations;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Domain.Services.Scheduling;

public sealed record ScheduledReservation(
    ReservationId ReservationId,
    TableId TableId,
    ScheduleSlot ScheduleSlot,
    ReservationStatus Status);
