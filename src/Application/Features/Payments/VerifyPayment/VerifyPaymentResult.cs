using EazyMenu.Domain.Aggregates.Payments;

namespace EazyMenu.Application.Features.Payments.VerifyPayment;

public sealed record VerifyPaymentResult(
    bool IsSuccessful,
    PaymentStatus PaymentStatus,
    string? ReferenceCode,
    string? FailureReason);
