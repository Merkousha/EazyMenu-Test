using System;
using System.Collections.Generic;
using EazyMenu.Domain.Common;
using EazyMenu.Domain.Common.Exceptions;
using EazyMenu.Domain.Common.Guards;

namespace EazyMenu.Domain.ValueObjects;

public sealed class ScheduleSlot : ValueObject
{
    public DayOfWeek DayOfWeek { get; }
    public TimeSpan Start { get; }
    public TimeSpan End { get; }

    private ScheduleSlot(DayOfWeek dayOfWeek, TimeSpan start, TimeSpan end)
    {
        if (!Enum.IsDefined(typeof(DayOfWeek), dayOfWeek))
        {
            throw new DomainException("روز هفته معتبر نیست.");
        }

        if (start < TimeSpan.Zero || start >= TimeSpan.FromDays(1))
        {
            throw new DomainException("زمان شروع معتبر نیست.");
        }

        if (end <= start || end > TimeSpan.FromDays(1))
        {
            throw new DomainException("زمان پایان باید بعد از شروع و حداکثر تا ۲۴ ساعت باشد.");
        }

        DayOfWeek = dayOfWeek;
        Start = start;
        End = end;
    }

    public static ScheduleSlot Create(DayOfWeek dayOfWeek, TimeSpan start, TimeSpan end)
    {
        return new ScheduleSlot(dayOfWeek, start, end);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return DayOfWeek;
        yield return Start;
        yield return End;
    }
}
