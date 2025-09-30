namespace EazyMenu.Application.Features.Customers.Login;

public sealed record VerifyCustomerLoginResult(bool IsAuthenticated, string PhoneNumber, string? FailureReason = null);
