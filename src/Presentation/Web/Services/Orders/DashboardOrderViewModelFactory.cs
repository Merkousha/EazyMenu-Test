using System;
using System.Linq;
using EazyMenu.Application.Features.Orders.Common;
using EazyMenu.Domain.ValueObjects;
using EazyMenu.Web.ViewModels.Orders;

namespace EazyMenu.Web.Services.Orders;

public class DashboardOrderViewModelFactory
{
    public OrderListViewModel CreateListViewModel(
        IReadOnlyCollection<OrderSummaryDto> orders,
        int pageNumber,
        int pageSize,
        string? statusFilter)
    {
        return new OrderListViewModel
        {
            Orders = orders.Select(ToSummaryViewModel).ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            StatusFilter = statusFilter
        };
    }

    public OrderSummaryViewModel ToSummaryViewModel(OrderSummaryDto dto)
    {
        return new OrderSummaryViewModel
        {
            OrderId = dto.OrderId,
            OrderNumber = dto.OrderNumber,
            Status = GetStatusDisplayName(dto.Status),
            StatusBadgeClass = GetStatusBadgeClass(dto.Status),
            FulfillmentMethod = GetFulfillmentMethodDisplayName(dto.FulfillmentMethod),
            FulfillmentMethodBadge = GetFulfillmentMethodBadge(dto.FulfillmentMethod),
            TotalAmount = dto.TotalAmount,
            CreatedAt = dto.CreatedAtUtc.ToLocalTime(),
            CustomerName = dto.CustomerName,
            CustomerPhone = dto.CustomerPhone,
            RelativeTime = GetRelativeTime(dto.CreatedAtUtc)
        };
    }

    public OrderDetailsViewModel ToDetailsViewModel(OrderDetailsDto dto)
    {
        return new OrderDetailsViewModel
        {
            OrderId = dto.OrderId,
            OrderNumber = dto.OrderNumber,
            Status = GetStatusDisplayName(dto.Status),
            StatusBadgeClass = GetStatusBadgeClass(dto.Status),
            FulfillmentMethod = GetFulfillmentMethodDisplayName(dto.FulfillmentMethod),
            CustomerName = dto.CustomerName,
            CustomerPhone = dto.CustomerPhone,
            CustomerNote = dto.CustomerNote,
            SubtotalAmount = dto.SubtotalAmount,
            TaxAmount = dto.TaxAmount,
            DeliveryFee = dto.DeliveryFee,
            TotalAmount = dto.TotalAmount,
            CreatedAt = dto.CreatedAtUtc.ToLocalTime(),
            ConfirmedAt = dto.ConfirmedAtUtc?.ToLocalTime(),
            CompletedAt = dto.CompletedAtUtc?.ToLocalTime(),
            CancelledAt = dto.CancelledAtUtc?.ToLocalTime(),
            CancellationReason = dto.CancellationReason,
            Items = dto.Items.Select(ToItemViewModel).ToList()
        };
    }

    private OrderItemViewModel ToItemViewModel(OrderItemDto dto)
    {
        return new OrderItemViewModel
        {
            OrderItemId = dto.OrderItemId,
            DisplayName = dto.DisplayName,
            UnitPrice = dto.UnitPrice,
            Quantity = dto.Quantity,
            TotalAmount = dto.TotalAmount,
            Note = dto.Note
        };
    }

    private string GetStatusDisplayName(OrderStatus status) => status switch
    {
        OrderStatus.Pending => "در انتظار",
        OrderStatus.Confirmed => "تأیید شده",
        OrderStatus.Completed => "تکمیل شده",
        OrderStatus.Cancelled => "لغو شده",
        _ => status.ToString()
    };

    private string GetStatusBadgeClass(OrderStatus status) => status switch
    {
        OrderStatus.Pending => "badge bg-warning text-dark",
        OrderStatus.Confirmed => "badge bg-info",
        OrderStatus.Completed => "badge bg-success",
        OrderStatus.Cancelled => "badge bg-danger",
        _ => "badge bg-secondary"
    };

    private string GetFulfillmentMethodDisplayName(FulfillmentMethod method) => method switch
    {
        FulfillmentMethod.DineIn => "حضوری",
        FulfillmentMethod.Pickup => "بیرون‌بر",
        FulfillmentMethod.Delivery => "ارسال",
        _ => method.ToString()
    };

    private string GetFulfillmentMethodBadge(FulfillmentMethod method) => method switch
    {
        FulfillmentMethod.DineIn => "badge bg-primary",
        FulfillmentMethod.Pickup => "badge bg-secondary",
        FulfillmentMethod.Delivery => "badge bg-success",
        _ => "badge bg-light text-dark"
    };

    private string GetRelativeTime(DateTime utcTime)
    {
        var span = DateTime.UtcNow - utcTime;
        
        if (span.TotalMinutes < 1)
            return "همین الان";
        if (span.TotalMinutes < 60)
            return $"{(int)span.TotalMinutes} دقیقه پیش";
        if (span.TotalHours < 24)
            return $"{(int)span.TotalHours} ساعت پیش";
        if (span.TotalDays < 7)
            return $"{(int)span.TotalDays} روز پیش";
        
        return utcTime.ToLocalTime().ToString("yyyy/MM/dd");
    }
}
