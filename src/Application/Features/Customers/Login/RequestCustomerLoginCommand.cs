using System;
using EazyMenu.Application.Abstractions.Messaging;

namespace EazyMenu.Application.Features.Customers.Login;

public sealed record RequestCustomerLoginCommand(string PhoneNumber, Guid TenantId) : ICommand<RequestCustomerLoginResult>;
