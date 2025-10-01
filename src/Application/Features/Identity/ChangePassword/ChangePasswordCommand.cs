using System;
using EazyMenu.Application.Abstractions.Messaging;

namespace EazyMenu.Application.Features.Identity.ChangePassword;

/// <summary>
/// فرمان تغییر رمز عبور کاربر.
/// </summary>
public sealed record ChangePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword) : ICommand<bool>;
