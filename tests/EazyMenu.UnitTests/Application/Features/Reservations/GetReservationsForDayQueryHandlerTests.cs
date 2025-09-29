using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Features.Reservations.GetReservationsForDay;
using EazyMenu.Domain.Aggregates.Reservations;
using EazyMenu.Domain.ValueObjects;
using Xunit;

namespace EazyMenu.UnitTests.Application.Features.Reservations;

public class GetReservationsForDayQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldReturnReservationsOrderedAndMapped()
    {
        var tenantId = TenantId.New();
        var branchId = BranchId.New();
        var tableId1 = TableId.New();
        var tableId2 = TableId.New();

        var earlySlot = ScheduleSlot.Create(DayOfWeek.Monday, new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0));
        var lateSlot = ScheduleSlot.Create(DayOfWeek.Monday, new TimeSpan(18, 0, 0), new TimeSpan(19, 0, 0));

        var laterReservation = Reservation.Schedule(
            tenantId,
            branchId,
            tableId1,
            lateSlot,
            partySize: 2,
            tableCapacity: 4,
            specialRequest: "Birthday",
            customerName: "Ali");

        var earlierReservation = Reservation.Schedule(
            tenantId,
            branchId,
            tableId2,
            earlySlot,
            partySize: 3,
            tableCapacity: 4,
            specialRequest: "Window",
            customerName: "Sara");

        var repository = new FakeReservationRepository
        {
            ReservationsToReturn = new[] { laterReservation, earlierReservation }
        };

        var handler = new GetReservationsForDayQueryHandler(repository);
        var query = new GetReservationsForDayQuery(tenantId.Value, branchId.Value, DayOfWeek.Monday);

        var result = await handler.HandleAsync(query, CancellationToken.None);
        var list = result.ToList();

        Assert.Equal(tenantId, repository.LastTenantId);
        Assert.Equal(branchId, repository.LastBranchId);
        Assert.Equal(DayOfWeek.Monday, repository.LastDayOfWeek);

        Assert.Collection(
            list,
            first =>
            {
                Assert.Equal(earlierReservation.Id.Value, first.ReservationId);
                Assert.Equal(tenantId.Value, first.TenantId);
                Assert.Equal(branchId.Value, first.BranchId);
                Assert.Equal(tableId2.Value, first.TableId);
                Assert.Equal(earlySlot.DayOfWeek, first.DayOfWeek);
                Assert.Equal(earlySlot.Start, first.Start);
                Assert.Equal(earlySlot.End, first.End);
                Assert.Equal(earlierReservation.PartySize, first.PartySize);
                Assert.Equal(earlierReservation.Status.ToString(), first.Status);
                Assert.Equal("Sara", first.CustomerName);
                Assert.Equal("Window", first.SpecialRequest);
            },
            second =>
            {
                Assert.Equal(laterReservation.Id.Value, second.ReservationId);
                Assert.Equal(tenantId.Value, second.TenantId);
                Assert.Equal(branchId.Value, second.BranchId);
                Assert.Equal(tableId1.Value, second.TableId);
                Assert.Equal(lateSlot.DayOfWeek, second.DayOfWeek);
                Assert.Equal(lateSlot.Start, second.Start);
                Assert.Equal(lateSlot.End, second.End);
                Assert.Equal(laterReservation.PartySize, second.PartySize);
                Assert.Equal(laterReservation.Status.ToString(), second.Status);
                Assert.Equal("Ali", second.CustomerName);
                Assert.Equal("Birthday", second.SpecialRequest);
            });
    }

    [Fact]
    public async Task HandleAsync_WithInvalidTenantId_ShouldThrowBusinessRuleViolationException()
    {
        var repository = new FakeReservationRepository();
        var handler = new GetReservationsForDayQueryHandler(repository);
        var query = new GetReservationsForDayQuery(Guid.Empty, Guid.NewGuid(), DayOfWeek.Tuesday);

        await Assert.ThrowsAsync<BusinessRuleViolationException>(() => handler.HandleAsync(query, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WithInvalidBranchId_ShouldThrowBusinessRuleViolationException()
    {
        var repository = new FakeReservationRepository();
        var handler = new GetReservationsForDayQueryHandler(repository);
        var query = new GetReservationsForDayQuery(Guid.NewGuid(), Guid.Empty, DayOfWeek.Wednesday);

        await Assert.ThrowsAsync<BusinessRuleViolationException>(() => handler.HandleAsync(query, CancellationToken.None));
    }

    private sealed class FakeReservationRepository : IReservationRepository
    {
        public IReadOnlyCollection<Reservation> ReservationsToReturn { get; set; } = Array.Empty<Reservation>();

        public TenantId LastTenantId { get; private set; }

        public BranchId LastBranchId { get; private set; }

        public DayOfWeek LastDayOfWeek { get; private set; }

        public CancellationToken LastCancellationToken { get; private set; }

        public Task<Reservation?> GetByIdAsync(ReservationId reservationId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<Reservation>> GetByBranchAndDayAsync(TenantId tenantId, BranchId branchId, DayOfWeek dayOfWeek, CancellationToken cancellationToken = default)
        {
            LastTenantId = tenantId;
            LastBranchId = branchId;
            LastDayOfWeek = dayOfWeek;
            LastCancellationToken = cancellationToken;

            return Task.FromResult(ReservationsToReturn);
        }

        public Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Reservation reservation, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
