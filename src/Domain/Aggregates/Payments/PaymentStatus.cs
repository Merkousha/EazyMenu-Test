namespace EazyMenu.Domain.Aggregates.Payments;

public enum PaymentStatus
{
    Pending = 0,
    Succeeded = 1,
    Failed = 2,
    Refunded = 3
}
