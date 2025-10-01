namespace EazyMenu.Domain.ValueObjects;

public enum OrderStatus : byte
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2,
    Completed = 3
}
