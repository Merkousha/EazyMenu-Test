using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Common.Interfaces.Payments;

public sealed record PaymentVerificationRequest(string Authority, Money Amount);
