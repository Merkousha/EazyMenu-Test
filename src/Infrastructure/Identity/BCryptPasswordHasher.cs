using EazyMenu.Application.Common.Interfaces.Identity;

namespace EazyMenu.Infrastructure.Identity;

/// <summary>
/// پیاده‌سازی هش کردن رمز عبور با استفاده از BCrypt.
/// </summary>
public sealed class BCryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12; // BCrypt work factor (higher = more secure but slower)

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("رمز عبور نمی‌تواند خالی باشد.", nameof(password));
        }

        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword))
        {
            throw new ArgumentException("رمز عبور هش شده نمی‌تواند خالی باشد.", nameof(hashedPassword));
        }

        if (string.IsNullOrWhiteSpace(providedPassword))
        {
            return false;
        }

        try
        {
            return BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword);
        }
        catch
        {
            return false;
        }
    }
}
