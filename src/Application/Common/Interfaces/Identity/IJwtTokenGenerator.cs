using System;
using System.Collections.Generic;
using EazyMenu.Domain.Aggregates.Users;

namespace EazyMenu.Application.Common.Interfaces.Identity;

/// <summary>
/// سرویس تولید و اعتبارسنجی توکن‌های JWT.
/// </summary>
public interface IJwtTokenGenerator
{
    /// <summary>
    /// تولید Access Token برای کاربر.
    /// </summary>
    /// <param name="userId">شناسه کاربر</param>
    /// <param name="tenantId">شناسه مستاجر</param>
    /// <param name="email">ایمیل کاربر</param>
    /// <param name="role">نقش کاربر</param>
    /// <param name="claims">claims اضافی</param>
    /// <returns>Access Token</returns>
    string GenerateAccessToken(
        Guid userId,
        Guid tenantId,
        string email,
        UserRole role,
        Dictionary<string, string>? claims = null);
    
    /// <summary>
    /// تولید Refresh Token برای کاربر.
    /// </summary>
    /// <returns>Refresh Token</returns>
    string GenerateRefreshToken();
    
    /// <summary>
    /// اعتبارسنجی و دریافت اطلاعات از توکن.
    /// </summary>
    /// <param name="token">توکن برای اعتبارسنجی</param>
    /// <returns>اطلاعات کاربر یا null</returns>
    (Guid UserId, Guid TenantId, string Email, UserRole Role)? ValidateToken(string token);
}
