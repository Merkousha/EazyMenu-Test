using System;
using System.Collections.Generic;

namespace EazyMenu.Application.Features.Tenants.Common;

public sealed record BranchDetailsDto(
    Guid BranchId,
    string Name,
    string City,
    string Street,
    string PostalCode,
    IReadOnlyCollection<WorkingHourDto> WorkingHours,
    IReadOnlyCollection<BranchTableDto> Tables);
