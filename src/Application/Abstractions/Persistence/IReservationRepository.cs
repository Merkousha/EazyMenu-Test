using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Domain.Aggregates.Reservations;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Abstractions.Persistence;

public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(ReservationId reservationId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Reservation>> GetByBranchAndDayAsync(TenantId tenantId, BranchId branchId, DayOfWeek dayOfWeek, CancellationToken cancellationToken = default);

    Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default);

    Task UpdateAsync(Reservation reservation, CancellationToken cancellationToken = default);
}
