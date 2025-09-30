namespace EazyMenu.Application.Common.Interfaces.Security;

/// <summary>
/// سازنده‌ی کدهای یکبارمصرف عددی جهت احراز هویت پیامکی.
/// </summary>
public interface IOneTimePasswordGenerator
{
    string GenerateNumericCode(int length);
}
