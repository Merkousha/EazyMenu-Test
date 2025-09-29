using System;
using EazyMenu.Application.Common.Interfaces;

namespace EazyMenu.Application.Common.Time;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
