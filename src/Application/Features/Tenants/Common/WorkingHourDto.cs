using System;

namespace EazyMenu.Application.Features.Tenants.Common;

public sealed record WorkingHourDto(DayOfWeek DayOfWeek, TimeSpan Start, TimeSpan End);
