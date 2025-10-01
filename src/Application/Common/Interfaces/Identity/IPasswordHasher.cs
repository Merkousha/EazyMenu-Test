namespace EazyMenu.Application.Common.Interfaces.Identity;

/// <summary>
/// سرویس هش کردن و اعتبارسنجی رمز عبور.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// هش کردن رمز عبور با الگوریتم امن.
    /// </summary>
    /// <param name="password">رمز عبور plain text</param>
    /// <returns>رمز عبور هش شده</returns>
    string HashPassword(string password);
    
    /// <summary>
    /// اعتبارسنجی رمز عبور با مقایسه با هش ذخیره شده.
    /// </summary>
    /// <param name="hashedPassword">رمز عبور هش شده ذخیره شده</param>
    /// <param name="providedPassword">رمز عبور وارد شده توسط کاربر</param>
    /// <returns>true اگر رمز عبور صحیح باشد</returns>
    bool VerifyPassword(string hashedPassword, string providedPassword);
}
