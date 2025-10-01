using System;
using EazyMenu.Application.Abstractions.Messaging;

namespace EazyMenu.Application.Features.Orders.CancelOrder;

public sealed record CancelOrderCommand(Guid TenantId, Guid OrderId, string? Reason) : ICommand<bool>;
