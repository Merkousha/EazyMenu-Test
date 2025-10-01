using System;
using EazyMenu.Application.Abstractions.Messaging;

namespace EazyMenu.Application.Features.Orders.ConfirmOrder;

public sealed record ConfirmOrderCommand(Guid TenantId, Guid OrderId) : ICommand<bool>;
