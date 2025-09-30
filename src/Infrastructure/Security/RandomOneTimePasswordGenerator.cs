using System;
using System.Security.Cryptography;
using EazyMenu.Application.Common.Interfaces.Security;

namespace EazyMenu.Infrastructure.Security;

internal sealed class RandomOneTimePasswordGenerator : IOneTimePasswordGenerator
{
    public string GenerateNumericCode(int length)
    {
        if (length <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "طول کد باید بزرگ‌تر از صفر باشد.");
        }

        var buffer = new byte[length];
        RandomNumberGenerator.Fill(buffer);

        var digits = new char[length];
        for (var i = 0; i < length; i++)
        {
            digits[i] = (char)('0' + buffer[i] % 10);
        }

        return new string(digits);
    }
}
