using System;
using System.Collections.Generic;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Features.Reservations.Common;

namespace EazyMenu.Application.Features.Reservations.GetReservationsForDay;

public sealed record GetReservationsForDayQuery(
    Guid TenantId,
    Guid BranchId,
    DayOfWeek DayOfWeek) : IQuery<IReadOnlyCollection<ReservationSummaryDto>>;
