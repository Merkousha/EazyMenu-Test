using EazyMenu.Domain.Abstractions;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Domain.Events;

public sealed record ReservationCreatedDomainEvent(
    ReservationId ReservationId,
    TenantId TenantId,
    BranchId BranchId,
    TableId TableId,
    ScheduleSlot ScheduleSlot,
    int PartySize) : DomainEventBase;
