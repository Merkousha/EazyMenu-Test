using System;
using System.Collections.Generic;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Features.Tenants.Common;

namespace EazyMenu.Application.Features.Tenants.Branches.UpdateBranchWorkingHours;

public sealed record UpdateBranchWorkingHoursCommand(
    Guid TenantId,
    Guid BranchId,
    IReadOnlyCollection<WorkingHourDto> WorkingHours) : ICommand<bool>;
