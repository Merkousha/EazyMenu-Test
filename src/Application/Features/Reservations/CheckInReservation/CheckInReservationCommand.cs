using System;
using EazyMenu.Application.Abstractions.Messaging;

namespace EazyMenu.Application.Features.Reservations.CheckInReservation;

public sealed record CheckInReservationCommand(
    Guid ReservationId,
    Guid TenantId,
    Guid BranchId,
    string? Note) : ICommand<bool>;
