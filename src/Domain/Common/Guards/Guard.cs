using System;

namespace EazyMenu.Domain.Common.Guards;

public static class Guard
{
    public static void AgainstNull(object? value, string name)
    {
        if (value is null)
        {
            throw new ArgumentNullException(name);
        }
    }

    public static void AgainstNullOrWhiteSpace(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{name} نمی‌تواند خالی باشد.", name);
        }
    }

    public static void AgainstNegative(decimal value, string name)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(name, value, $"{name} نمی‌تواند منفی باشد.");
        }
    }

    public static void AgainstOutOfRange(int value, int minInclusive, int maxInclusive, string name)
    {
        if (value < minInclusive || value > maxInclusive)
        {
            throw new ArgumentOutOfRangeException(name, value, $"{name} باید بین {minInclusive} و {maxInclusive} باشد.");
        }
    }

    public static void AgainstDefault<T>(T value, string name) where T : struct
    {
        if (Equals(value, default(T)))
        {
            throw new ArgumentException($"{name} مقدار معتبر ندارد.", name);
        }
    }
}
