using System;

namespace EazyMenu.Public.ViewModels.Cart;

/// <summary>
/// ViewModel for the order confirmation page after successful order placement.
/// </summary>
public sealed class OrderConfirmationViewModel
{
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string FulfillmentMethod { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public string EstimatedReadyTime { get; set; } = string.Empty;
}
