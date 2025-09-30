namespace EazyMenu.Application.Common.Interfaces.Security;

public sealed record OneTimePasswordValidationResult(bool IsValid, bool IsExpired, string? FailureReason = null)
{
    public static OneTimePasswordValidationResult Success() => new(true, false, null);

    public static OneTimePasswordValidationResult Invalid(string? reason = null) => new(false, false, reason);

    public static OneTimePasswordValidationResult Expired(string? reason = null) => new(false, true, reason);
}
