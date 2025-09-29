using System;
using EazyMenu.Application.Abstractions.Messaging;

namespace EazyMenu.Application.Features.Reservations.MarkReservationNoShow;

public sealed record MarkReservationNoShowCommand(
    Guid ReservationId,
    Guid TenantId,
    Guid BranchId,
    string? Note) : ICommand<bool>;
