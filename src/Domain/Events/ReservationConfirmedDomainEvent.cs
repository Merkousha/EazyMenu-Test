using System;
using EazyMenu.Domain.Abstractions;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Domain.Events;

public sealed record ReservationConfirmedDomainEvent(
    ReservationId ReservationId,
    TenantId TenantId,
    BranchId BranchId,
    TableId TableId,
    DateTime ConfirmedAtUtc) : DomainEventBase;
