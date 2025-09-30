namespace EazyMenu.Application.Common.Interfaces.Payments;

public sealed record PaymentVerificationResponse(bool IsSuccessful, string? ReferenceId, string? FailureReason);
