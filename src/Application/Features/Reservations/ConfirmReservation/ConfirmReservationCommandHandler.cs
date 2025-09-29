using System;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Common.Interfaces;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Reservations.ConfirmReservation;

public sealed class ConfirmReservationCommandHandler : ICommandHandler<ConfirmReservationCommand, bool>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ConfirmReservationCommandHandler(
        IReservationRepository reservationRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _reservationRepository = reservationRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<bool> HandleAsync(ConfirmReservationCommand command, CancellationToken cancellationToken = default)
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

        EnsureReservationBelongsTo(command, reservation);

        reservation.Confirm(_dateTimeProvider.UtcNow, command.Note);

        await _reservationRepository.UpdateAsync(reservation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static void EnsureReservationBelongsTo(ConfirmReservationCommand command, Domain.Aggregates.Reservations.Reservation reservation)
    {
        if (reservation.TenantId.Value != command.TenantId || reservation.BranchId.Value != command.BranchId)
        {
            throw new BusinessRuleViolationException("رزرو به مستاجر یا شعبه دیگری تعلق دارد.");
        }
    }
}
