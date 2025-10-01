using System;
using System.Collections.Generic;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Web.ViewModels.Orders;

public sealed class OrderListViewModel
{
    public IReadOnlyCollection<OrderSummaryViewModel> Orders { get; init; } = Array.Empty<OrderSummaryViewModel>();
    
    public int PageNumber { get; init; } = 1;
    
    public int PageSize { get; init; } = 20;
    
    public string? StatusFilter { get; init; }
    
    public bool HasOrders => Orders.Count > 0;
}

public sealed class OrderSummaryViewModel
{
    public Guid OrderId { get; init; }
    
    public string OrderNumber { get; init; } = string.Empty;
    
    public string Status { get; init; } = string.Empty;
    
    public string StatusBadgeClass { get; init; } = string.Empty;
    
    public string FulfillmentMethod { get; init; } = string.Empty;
    
    public string FulfillmentMethodBadge { get; init; } = string.Empty;
    
    public decimal TotalAmount { get; init; }
    
    public DateTime CreatedAt { get; init; }
    
    public string CustomerName { get; init; } = string.Empty;
    
    public string CustomerPhone { get; init; } = string.Empty;
    
    public string RelativeTime { get; init; } = string.Empty;
}
