using System;
using EazyMenu.Domain.Abstractions;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Domain.Events;

public sealed record ReservationNoShowRecordedDomainEvent(
    ReservationId ReservationId,
    TenantId TenantId,
    BranchId BranchId,
    TableId TableId,
    DateTime RecordedAtUtc) : DomainEventBase;
