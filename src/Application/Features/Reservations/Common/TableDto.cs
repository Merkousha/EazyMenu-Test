using System;

namespace EazyMenu.Application.Features.Reservations.Common;

public sealed record TableDto(
    Guid TableId,
    string Label,
    int Capacity,
    bool IsOutdoor,
    bool IsOutOfService);
