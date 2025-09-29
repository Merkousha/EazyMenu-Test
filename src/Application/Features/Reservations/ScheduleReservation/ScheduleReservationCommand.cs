using System;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Reservations.ScheduleReservation;

public sealed record ScheduleReservationCommand(
    Guid TenantId,
    Guid BranchId,
    DayOfWeek DayOfWeek,
    TimeSpan Start,
    TimeSpan End,
    int PartySize,
    bool PrefersOutdoor,
    string? SpecialRequest,
    string? CustomerName,
    string? CustomerPhone) : ICommand<ReservationId>;
