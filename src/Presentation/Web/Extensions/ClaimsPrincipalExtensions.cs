using System;
using System.Security.Claims;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Web.Extensions;

/// <summary>
/// توابع کمکی برای کار با Claims کاربر.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// استخراج UserId از Claims کاربر.
    /// </summary>
    public static UserId? GetUserId(this ClaimsPrincipal principal)
    {
        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userIdGuid))
        {
            return null;
        }

        return UserId.From(userIdGuid);
    }

    /// <summary>
    /// استخراج TenantId از Claims کاربر.
    /// </summary>
    public static TenantId? GetTenantId(this ClaimsPrincipal principal)
    {
        var tenantIdClaim = principal.FindFirst("TenantId")?.Value;
        if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantIdGuid))
        {
            return null;
        }

        return TenantId.FromGuid(tenantIdGuid);
    }

    /// <summary>
    /// دریافت نقش کاربر.
    /// </summary>
    public static string? GetUserRole(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Role)?.Value;
    }

    /// <summary>
    /// دریافت ایمیل کاربر.
    /// </summary>
    public static string? GetUserEmail(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Email)?.Value;
    }

    /// <summary>
    /// بررسی اینکه آیا کاربر Owner است یا خیر.
    /// </summary>
    public static bool IsOwner(this ClaimsPrincipal principal)
    {
        return principal.IsInRole("Owner");
    }

    /// <summary>
    /// بررسی اینکه آیا کاربر Manager یا Owner است.
    /// </summary>
    public static bool IsManagerOrAbove(this ClaimsPrincipal principal)
    {
        var role = principal.GetUserRole();
        return role == "Owner" || role == "Manager";
    }
}
