namespace EazyMenu.Domain.Aggregates.Reservations;

public enum ReservationStatus
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2,
    CheckedIn = 3,
    NoShow = 4
}
