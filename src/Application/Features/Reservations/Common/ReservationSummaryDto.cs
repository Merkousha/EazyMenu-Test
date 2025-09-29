using System;

namespace EazyMenu.Application.Features.Reservations.Common;

public sealed record ReservationSummaryDto(
    Guid ReservationId,
    Guid TenantId,
    Guid BranchId,
    Guid TableId,
    DayOfWeek DayOfWeek,
    TimeSpan Start,
    TimeSpan End,
    int PartySize,
    string Status,
    string? CustomerName,
    string? SpecialRequest);
