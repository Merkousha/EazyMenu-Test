using System;
using System.Threading;
using System.Threading.Tasks;

namespace EazyMenu.Application.Common.Interfaces.Security;

/// <summary>
/// ذخیره‌سازی و اعتبارسنجی کدهای یکبارمصرف برای ورود کاربران.
/// </summary>
public interface IOneTimePasswordStore
{
    Task StoreAsync(string phoneNumber, string code, DateTime expiresAtUtc, CancellationToken cancellationToken = default);

    Task<OneTimePasswordValidationResult> ValidateAsync(string phoneNumber, string code, CancellationToken cancellationToken = default);
}
