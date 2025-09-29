using System;
using EazyMenu.Domain.Abstractions;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Domain.Events;

public sealed record ReservationCancelledDomainEvent(
    ReservationId ReservationId,
    TenantId TenantId,
    BranchId BranchId,
    TableId TableId,
    DateTime CancelledAtUtc,
    string Reason) : DomainEventBase;
