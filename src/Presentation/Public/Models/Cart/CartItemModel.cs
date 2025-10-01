using System;

namespace EazyMenu.Public.Models.Cart;

/// <summary>
/// Represents a single item in the shopping cart.
/// </summary>
public sealed class CartItemModel
{
    public Guid MenuItemId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public string? Note { get; set; }
    
    /// <summary>
    /// Gets the total price for this cart item (UnitPrice * Quantity).
    /// </summary>
    public decimal TotalPrice => UnitPrice * Quantity;
}
