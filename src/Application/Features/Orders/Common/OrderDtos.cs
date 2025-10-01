using System;
using System.Collections.Generic;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Orders.Common;

public sealed record OrderDto(
    Guid OrderId,
    Guid TenantId,
    Guid MenuId,
    string OrderNumber,
    OrderStatus Status,
    FulfillmentMethod FulfillmentMethod,
    string CustomerName,
    string CustomerPhone,
    string? CustomerNote,
    decimal SubtotalAmount,
    decimal TaxAmount,
    decimal DeliveryFee,
    decimal TotalAmount,
    DateTime CreatedAtUtc,
    DateTime? ConfirmedAtUtc,
    DateTime? CompletedAtUtc,
    DateTime? CancelledAtUtc,
    string? CancellationReason,
    IReadOnlyCollection<OrderItemDto> Items);

public sealed record OrderItemDto(
    Guid OrderItemId,
    Guid MenuItemId,
    string DisplayName,
    decimal UnitPrice,
    int Quantity,
    decimal TotalAmount,
    string? Note);

public sealed record OrderSummaryDto(
    Guid OrderId,
    string OrderNumber,
    OrderStatus Status,
    FulfillmentMethod FulfillmentMethod,
    decimal TotalAmount,
    DateTime CreatedAtUtc,
    string CustomerName,
    string CustomerPhone);

public sealed record OrderDetailsDto(
    Guid OrderId,
    Guid TenantId,
    Guid MenuId,
    string OrderNumber,
    OrderStatus Status,
    FulfillmentMethod FulfillmentMethod,
    string CustomerName,
    string CustomerPhone,
    string? CustomerNote,
    decimal SubtotalAmount,
    decimal TaxAmount,
    decimal DeliveryFee,
    decimal TotalAmount,
    DateTime CreatedAtUtc,
    DateTime? ConfirmedAtUtc,
    DateTime? CompletedAtUtc,
    DateTime? CancelledAtUtc,
    string? CancellationReason,
    IReadOnlyCollection<OrderItemDto> Items);

public sealed record CreateOrderResultDto(
    Guid OrderId,
    string OrderNumber,
    OrderStatus Status,
    decimal TotalAmount);
