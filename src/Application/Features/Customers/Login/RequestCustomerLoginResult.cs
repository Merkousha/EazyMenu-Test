using System;

namespace EazyMenu.Application.Features.Customers.Login;

public sealed record RequestCustomerLoginResult(string PhoneNumber, DateTime ExpiresAtUtc, string DeliveryChannel);
