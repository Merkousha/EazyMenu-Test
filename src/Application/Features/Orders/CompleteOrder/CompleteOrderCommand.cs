using System;
using EazyMenu.Application.Abstractions.Messaging;

namespace EazyMenu.Application.Features.Orders.CompleteOrder;

public sealed record CompleteOrderCommand(Guid TenantId, Guid OrderId) : ICommand<bool>;
