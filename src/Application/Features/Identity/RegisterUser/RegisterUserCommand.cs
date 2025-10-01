using System;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Domain.Aggregates.Users;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Identity.RegisterUser;

/// <summary>
/// فرمان ثبت کاربر جدید در سیستم.
/// </summary>
public sealed record RegisterUserCommand(
    Guid TenantId,
    string Email,
    string FullName,
    string PhoneNumber,
    string Password,
    UserRole Role) : ICommand<UserId>;
