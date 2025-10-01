using System;
using System.Collections.Generic;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Web.ViewModels.Orders;

public sealed class OrderDetailsViewModel
{
    public Guid OrderId { get; init; }
    
    public string OrderNumber { get; init; } = string.Empty;
    
    public string Status { get; init; } = string.Empty;
    
    public string StatusBadgeClass { get; init; } = string.Empty;
    
    public string FulfillmentMethod { get; init; } = string.Empty;
    
    public string CustomerName { get; init; } = string.Empty;
    
    public string CustomerPhone { get; init; } = string.Empty;
    
    public string? CustomerNote { get; init; }
    
    public decimal SubtotalAmount { get; init; }
    
    public decimal TaxAmount { get; init; }
    
    public decimal DeliveryFee { get; init; }
    
    public decimal TotalAmount { get; init; }
    
    public DateTime CreatedAt { get; init; }
    
    public DateTime? ConfirmedAt { get; init; }
    
    public DateTime? CompletedAt { get; init; }
    
    public DateTime? CancelledAt { get; init; }
    
    public string? CancellationReason { get; init; }
    
    public IReadOnlyCollection<OrderItemViewModel> Items { get; init; } = Array.Empty<OrderItemViewModel>();
    
    public bool CanConfirm => Status == "Pending";
    
    public bool CanComplete => Status == "Confirmed";
    
    public bool CanCancel => Status is "Pending" or "Confirmed";
}

public sealed class OrderItemViewModel
{
    public Guid OrderItemId { get; init; }
    
    public string DisplayName { get; init; } = string.Empty;
    
    public decimal UnitPrice { get; init; }
    
    public int Quantity { get; init; }
    
    public decimal TotalAmount { get; init; }
    
    public string? Note { get; init; }
}
