using System;
using EazyMenu.Domain.Aggregates.Users;

namespace EazyMenu.Application.Features.Identity.Login;

/// <summary>
/// DTO اطلاعات کاربر برای نمایش.
/// </summary>
public sealed record UserDto(
    Guid UserId,
    Guid TenantId,
    string Email,
    string FullName,
    string PhoneNumber,
    UserRole Role,
    UserStatus Status);
