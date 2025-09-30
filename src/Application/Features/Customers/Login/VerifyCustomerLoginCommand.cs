using EazyMenu.Application.Abstractions.Messaging;

namespace EazyMenu.Application.Features.Customers.Login;

public sealed record VerifyCustomerLoginCommand(string PhoneNumber, string Code) : ICommand<VerifyCustomerLoginResult>;
