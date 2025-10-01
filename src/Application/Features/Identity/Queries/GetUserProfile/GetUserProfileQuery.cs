using System;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Features.Identity.Login;

namespace EazyMenu.Application.Features.Identity.Queries.GetUserProfile;

/// <summary>
/// کوئری دریافت پروفایل کاربر.
/// </summary>
public sealed record GetUserProfileQuery(Guid UserId) : IQuery<UserDto>;
