using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Domain.Aggregates.Reservations;
using EazyMenu.Domain.Services.Scheduling;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Reservations.ScheduleReservation;

public sealed class ScheduleReservationCommandHandler : ICommandHandler<ScheduleReservationCommand, ReservationId>
{
    private readonly IBranchRepository _branchRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IReservationSchedulingPolicy _schedulingPolicy;
    private readonly IUnitOfWork _unitOfWork;

    public ScheduleReservationCommandHandler(
        IBranchRepository branchRepository,
        IReservationRepository reservationRepository,
        IReservationSchedulingPolicy schedulingPolicy,
        IUnitOfWork unitOfWork)
    {
        _branchRepository = branchRepository;
        _reservationRepository = reservationRepository;
        _schedulingPolicy = schedulingPolicy;
        _unitOfWork = unitOfWork;
    }

    public async Task<ReservationId> HandleAsync(ScheduleReservationCommand command, CancellationToken cancellationToken = default)
    {
        if (!TenantId.TryCreate(command.TenantId, out var tenantId))
        {
            throw new BusinessRuleViolationException("شناسه مستاجر معتبر نیست.");
        }

        if (!BranchId.TryCreate(command.BranchId, out var branchId))
        {
            throw new BusinessRuleViolationException("شناسه شعبه معتبر نیست.");
        }

        var branch = await _branchRepository.GetByIdAsync(tenantId, branchId, cancellationToken);
        if (branch is null)
        {
            throw new NotFoundException("شعبه مورد نظر یافت نشد.");
        }

        var tables = branch.Tables;
        var scheduleSlot = ScheduleSlot.Create(command.DayOfWeek, command.Start, command.End);

        var existingReservations = await _reservationRepository.GetByBranchAndDayAsync(tenantId, branchId, command.DayOfWeek, cancellationToken);
        var scheduledReservations = existingReservations
            .Select(reservation => new ScheduledReservation(
                reservation.Id,
                reservation.TableId,
                reservation.ScheduleSlot,
                reservation.Status))
            .ToList();

        var allocation = _schedulingPolicy.Allocate(tables, scheduledReservations, scheduleSlot, command.PartySize, command.PrefersOutdoor);
        if (!allocation.IsSuccessful || allocation.TableId is null)
        {
            throw new BusinessRuleViolationException(allocation.FailureReason ?? "امکان رزرو برای بازه زمانی انتخابی وجود ندارد.");
        }

        var table = tables.Single(t => t.Id == allocation.TableId.Value);
        var customerPhone = string.IsNullOrWhiteSpace(command.CustomerPhone) ? null : PhoneNumber.Create(command.CustomerPhone);
        var reservation = Reservation.Schedule(
            tenantId,
            branchId,
            allocation.TableId.Value,
            scheduleSlot,
            command.PartySize,
            table.Capacity,
            command.SpecialRequest,
            command.CustomerName,
            customerPhone);

        await _reservationRepository.AddAsync(reservation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return reservation.Id;
    }
}
