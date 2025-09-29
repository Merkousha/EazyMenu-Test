using System;

namespace EazyMenu.Application.Features.Tenants.Common;

public sealed record BranchTableDto(
    Guid TableId,
    string Label,
    int Capacity,
    bool IsOutdoor,
    bool IsOutOfService);
