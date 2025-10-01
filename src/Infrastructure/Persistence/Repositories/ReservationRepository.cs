using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Domain.Aggregates.Reservations;
using EazyMenu.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EazyMenu.Infrastructure.Persistence.Repositories;

internal sealed class ReservationRepository : IReservationRepository
{
    private readonly EazyMenuDbContext _dbContext;

    public ReservationRepository(EazyMenuDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Reservation?> GetByIdAsync(ReservationId reservationId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Reservations
            .FirstOrDefaultAsync(reservation => reservation.Id == reservationId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Reservation>> GetByBranchAndDayAsync(
        TenantId tenantId,
        BranchId branchId,
        DayOfWeek dayOfWeek,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Reservations
            .Where(reservation => reservation.TenantId == tenantId &&
                                   reservation.BranchId == branchId &&
                                   reservation.ScheduleSlot.DayOfWeek == dayOfWeek)
            .OrderBy(reservation => reservation.ScheduleSlot.Start)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        _dbContext.Reservations.Add(reservation);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        _dbContext.Reservations.Update(reservation);
        return Task.CompletedTask;
    }
}
