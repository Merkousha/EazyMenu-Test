using System;
using EazyMenu.Application.Abstractions.Messaging;

namespace EazyMenu.Application.Features.Reservations.ConfirmReservation;

public sealed record ConfirmReservationCommand(
    Guid ReservationId,
    Guid TenantId,
    Guid BranchId,
    string? Note) : ICommand<bool>;
