using System;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Common.Interfaces;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Reservations.CheckInReservation;

public sealed class CheckInReservationCommandHandler : ICommandHandler<CheckInReservationCommand, bool>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CheckInReservationCommandHandler(
        IReservationRepository reservationRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _reservationRepository = reservationRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<bool> HandleAsync(CheckInReservationCommand command, CancellationToken cancellationToken = default)
    {
        if (!ReservationId.TryCreate(command.ReservationId, out var reservationId))
        {
            throw new BusinessRuleViolationException("شناسه رزرو معتبر نیست.");
        }

        var reservation = await _reservationRepository.GetByIdAsync(reservationId, cancellationToken);
        if (reservation is null)
        {
            throw new NotFoundException("رزرو مورد نظر یافت نشد.");
        }

        if (reservation.TenantId.Value != command.TenantId || reservation.BranchId.Value != command.BranchId)
        {
            throw new BusinessRuleViolationException("رزرو به مستاجر یا شعبه دیگری تعلق دارد.");
        }

        reservation.MarkAsCheckedIn(_dateTimeProvider.UtcNow, command.Note);

        await _reservationRepository.UpdateAsync(reservation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
