using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Features.Reservations.GetReservationsForDay;
using EazyMenu.Application.Features.Reservations.ScheduleReservation;
using EazyMenu.Domain.Aggregates.Reservations;
using EazyMenu.Domain.Aggregates.Tenants;
using EazyMenu.Domain.Services.Scheduling;
using EazyMenu.Domain.ValueObjects;
using Xunit;

namespace EazyMenu.IntegrationTests.Application.Features.Reservations;

public class GetReservationsForDayQueryHandlerIntegrationTests
{
    [Fact]
    public async Task ScheduleReservationFlow_ShouldBeQueryableByDay()
    {
        var tenantId = TenantId.New();
        var branch = CreateBranchWithTables();

        var branchRepository = new InMemoryBranchRepository();
        branchRepository.Seed(tenantId, branch);

        var reservationRepository = new InMemoryReservationRepository();
        var schedulingPolicy = new DefaultReservationSchedulingPolicy();
        var unitOfWork = new FakeUnitOfWork();

        var scheduleHandler = new ScheduleReservationCommandHandler(
            branchRepository,
            reservationRepository,
            schedulingPolicy,
            unitOfWork);

        var earlyCommand = new ScheduleReservationCommand(
            tenantId.Value,
            branch.Id.Value,
            DayOfWeek.Monday,
            new TimeSpan(9, 0, 0),
            new TimeSpan(10, 0, 0),
            PartySize: 2,
            PrefersOutdoor: false,
            SpecialRequest: "Window",
            CustomerName: "Sara",
            CustomerPhone: null);

        var lateCommand = new ScheduleReservationCommand(
            tenantId.Value,
            branch.Id.Value,
            DayOfWeek.Monday,
            new TimeSpan(18, 0, 0),
            new TimeSpan(19, 0, 0),
            PartySize: 4,
            PrefersOutdoor: false,
            SpecialRequest: "Birthday",
            CustomerName: "Ali",
            CustomerPhone: null);

        var earlyReservationId = await scheduleHandler.HandleAsync(earlyCommand, CancellationToken.None);
        var lateReservationId = await scheduleHandler.HandleAsync(lateCommand, CancellationToken.None);

        var queryHandler = new GetReservationsForDayQueryHandler(reservationRepository);
        var query = new GetReservationsForDayQuery(tenantId.Value, branch.Id.Value, DayOfWeek.Monday);

        var results = (await queryHandler.HandleAsync(query, CancellationToken.None)).ToList();

        Assert.Equal(2, results.Count);

        Assert.Equal(earlyReservationId.Value, results[0].ReservationId);
        Assert.Equal(branch.Id.Value, results[0].BranchId);
        Assert.Equal("Sara", results[0].CustomerName);
        Assert.Equal("Window", results[0].SpecialRequest);
        Assert.Equal(new TimeSpan(9, 0, 0), results[0].Start);
        Assert.Equal(new TimeSpan(10, 0, 0), results[0].End);

        Assert.Equal(lateReservationId.Value, results[1].ReservationId);
        Assert.Equal(branch.Id.Value, results[1].BranchId);
        Assert.Equal("Ali", results[1].CustomerName);
        Assert.Equal("Birthday", results[1].SpecialRequest);
        Assert.Equal(new TimeSpan(18, 0, 0), results[1].Start);
        Assert.Equal(new TimeSpan(19, 0, 0), results[1].End);
    }

    private static Branch CreateBranchWithTables()
    {
        var address = Address.Create("تهران", "خیابان 1", "1234567890");
        var branch = Branch.Create("شعبه مرکزی", address);
        branch.AddTable("Hall-1", 4);
        branch.AddTable("Hall-2", 6, isOutdoor: true);
        return branch;
    }

    private sealed class InMemoryBranchRepository : IBranchRepository
    {
        private readonly ConcurrentDictionary<(TenantId TenantId, BranchId BranchId), Branch> _branches = new();

        public void Seed(TenantId tenantId, Branch branch)
        {
            _branches[(tenantId, branch.Id)] = branch;
        }

        public Task<Branch?> GetByIdAsync(TenantId tenantId, BranchId branchId, CancellationToken cancellationToken = default)
        {
            _branches.TryGetValue((tenantId, branchId), out var branch);
            return Task.FromResult(branch);
        }

        public Task UpdateAsync(Branch branch, CancellationToken cancellationToken = default)
        {
            var entry = _branches.Keys.FirstOrDefault(key => key.BranchId == branch.Id);
            if (entry != default)
            {
                _branches[entry] = branch;
            }

            return Task.CompletedTask;
        }
    }

    private sealed class InMemoryReservationRepository : IReservationRepository
    {
        private readonly List<Reservation> _reservations = new();
        private readonly object _sync = new();

        public Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default)
        {
            lock (_sync)
            {
                _reservations.Add(reservation);
            }

            return Task.CompletedTask;
        }

        public Task<Reservation?> GetByIdAsync(ReservationId reservationId, CancellationToken cancellationToken = default)
        {
            lock (_sync)
            {
                return Task.FromResult(_reservations.FirstOrDefault(r => r.Id == reservationId));
            }
        }

        public Task<IReadOnlyCollection<Reservation>> GetByBranchAndDayAsync(TenantId tenantId, BranchId branchId, DayOfWeek dayOfWeek, CancellationToken cancellationToken = default)
        {
            lock (_sync)
            {
                var matches = _reservations
                    .Where(reservation =>
                        reservation.TenantId == tenantId &&
                        reservation.BranchId == branchId &&
                        reservation.ScheduleSlot.DayOfWeek == dayOfWeek)
                    .ToList()
                    .AsReadOnly();

                return Task.FromResult<IReadOnlyCollection<Reservation>>(matches);
            }
        }

        public Task UpdateAsync(Reservation reservation, CancellationToken cancellationToken = default)
        {
            lock (_sync)
            {
                var index = _reservations.FindIndex(r => r.Id == reservation.Id);
                if (index >= 0)
                {
                    _reservations[index] = reservation;
                }
            }

            return Task.CompletedTask;
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
