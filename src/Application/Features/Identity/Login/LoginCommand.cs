using EazyMenu.Application.Abstractions.Messaging;

namespace EazyMenu.Application.Features.Identity.Login;

/// <summary>
/// فرمان ورود کاربر به سیستم.
/// </summary>
public sealed record LoginCommand(
    string Email,
    string Password) : ICommand<LoginResult>;

/// <summary>
/// نتیجه ورود کاربر.
/// </summary>
public sealed record LoginResult(
    bool IsSuccessful,
    string? AccessToken,
    string? RefreshToken,
    UserDto? User,
    string? ErrorMessage);
