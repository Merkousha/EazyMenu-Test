using System;

namespace EazyMenu.Application.Features.Tenants.Common;

public sealed record BranchSummaryDto(
    Guid BranchId,
    string Name,
    string City,
    string Street,
    string PostalCode,
    int TableCount,
    int WorkingHourCount);
