using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Features.Reservations.Common;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Reservations.GetReservationsForDay;

public sealed class GetReservationsForDayQueryHandler : IQueryHandler<GetReservationsForDayQuery, IReadOnlyCollection<ReservationSummaryDto>>
{
    private readonly IReservationRepository _reservationRepository;

    public GetReservationsForDayQueryHandler(IReservationRepository reservationRepository)
    {
        _reservationRepository = reservationRepository;
    }

    public async Task<IReadOnlyCollection<ReservationSummaryDto>> HandleAsync(GetReservationsForDayQuery query, CancellationToken cancellationToken = default)
    {
        if (!TenantId.TryCreate(query.TenantId, out var tenantId))
        {
            throw new BusinessRuleViolationException("شناسه مستاجر معتبر نیست.");
        }

        if (!BranchId.TryCreate(query.BranchId, out var branchId))
        {
            throw new BusinessRuleViolationException("شناسه شعبه معتبر نیست.");
        }

        var reservations = await _reservationRepository.GetByBranchAndDayAsync(tenantId, branchId, query.DayOfWeek, cancellationToken);

        return reservations
            .OrderBy(reservation => reservation.ScheduleSlot.Start)
            .Select(reservation => new ReservationSummaryDto(
                reservation.Id.Value,
                reservation.TenantId.Value,
                reservation.BranchId.Value,
                reservation.TableId.Value,
                reservation.ScheduleSlot.DayOfWeek,
                reservation.ScheduleSlot.Start,
                reservation.ScheduleSlot.End,
                reservation.PartySize,
                reservation.Status.ToString(),
                reservation.CustomerName,
                reservation.SpecialRequest))
            .ToList();
    }
}
