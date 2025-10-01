using System;
using System.Collections.Generic;

namespace EazyMenu.Web.Models.Reservations;

public sealed record ReservationListViewModel(
    Guid TenantId,
    Guid BranchId,
    DayOfWeek SelectedDay,
    IReadOnlyCollection<ReservationSummaryViewModel> Reservations);

public sealed record ReservationSummaryViewModel(
    Guid ReservationId,
    Guid TableId,
    string TimeSlot,
    int PartySize,
    string CustomerName,
    string Status,
    string? SpecialRequest);
