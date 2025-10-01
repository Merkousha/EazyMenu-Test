using System;
using System.Collections.Generic;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Orders.PlaceOrder;

public sealed record PlaceOrderCommand(
    Guid TenantId,
    Guid MenuId,
    string FulfillmentMethod,
    string CustomerName,
    string CustomerPhone,
    string? CustomerNote,
    decimal DeliveryFee,
    decimal TaxAmount,
    IReadOnlyCollection<OrderItemInput> Items) : ICommand<OrderId>;

public sealed record OrderItemInput(
    Guid MenuItemId,
    string DisplayName,
    decimal UnitPrice,
    int Quantity,
    string? Note);
