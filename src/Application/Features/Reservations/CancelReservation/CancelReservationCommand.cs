using System;
using EazyMenu.Application.Abstractions.Messaging;

namespace EazyMenu.Application.Features.Reservations.CancelReservation;

public sealed record CancelReservationCommand(
    Guid ReservationId,
    Guid TenantId,
    Guid BranchId,
    string Reason) : ICommand<bool>;
