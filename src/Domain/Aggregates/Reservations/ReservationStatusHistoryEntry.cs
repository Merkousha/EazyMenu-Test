using System;

namespace EazyMenu.Domain.Aggregates.Reservations;

public sealed class ReservationStatusHistoryEntry
{
    public ReservationStatusHistoryEntry(ReservationStatus status, DateTime changedAtUtc, string? note)
    {
        Status = status;
        ChangedAtUtc = changedAtUtc;
        Note = note;
    }

    private ReservationStatusHistoryEntry()
    {
    }

    public ReservationStatus Status { get; private set; }

    public DateTime ChangedAtUtc { get; private set; }

    public string? Note { get; private set; }
}
