using System.Collections.Generic;
using System.Linq;
using EazyMenu.Domain.Aggregates.Orders;

namespace EazyMenu.Application.Features.Orders.Common;

public static class OrderMapper
{
    public static OrderDto ToDto(Order order)
    {
        return new OrderDto(
            order.Id.Value,
            order.TenantId.Value,
            order.MenuId,
            order.OrderNumber,
            order.Status,
            order.FulfillmentMethod,
            order.CustomerName,
            order.CustomerPhone.ToString(),
            order.CustomerNote,
            order.SubtotalAmount,
            order.TaxAmount,
            order.DeliveryFee,
            order.TotalAmount,
            order.CreatedAtUtc,
            order.ConfirmedAtUtc,
            order.CompletedAtUtc,
            order.CancelledAtUtc,
            order.CancellationReason,
            order.Items.Select(ToDto).ToList());
    }

    public static OrderSummaryDto ToSummaryDto(Order order)
    {
        return new OrderSummaryDto(
            order.Id.Value,
            order.OrderNumber,
            order.Status,
            order.FulfillmentMethod,
            order.TotalAmount,
            order.CreatedAtUtc,
            order.CustomerName,
            order.CustomerPhone.ToString());
    }

    private static OrderItemDto ToDto(OrderItem item)
    {
        return new OrderItemDto(
            item.Id.Value,
            item.MenuItemId,
            item.DisplayName,
            item.UnitPrice,
            item.Quantity,
            item.TotalAmount,
            item.Note);
    }

    public static IReadOnlyCollection<OrderItemDto> ToItemDtos(Order order) => order.Items.Select(ToDto).ToList();
}
