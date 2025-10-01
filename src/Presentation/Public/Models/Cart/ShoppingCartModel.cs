using System;
using System.Collections.Generic;
using System.Linq;

namespace EazyMenu.Public.Models.Cart;

/// <summary>
/// Represents the customer's shopping cart with all items and calculations.
/// </summary>
public sealed class ShoppingCartModel
{
    public List<CartItemModel> Items { get; set; } = new();
    public Guid MenuId { get; set; }
    public Guid TenantId { get; set; }
    
    /// <summary>
    /// Gets the total number of items in the cart.
    /// </summary>
    public int TotalItemCount => Items.Sum(x => x.Quantity);
    
    /// <summary>
    /// Gets the subtotal amount (sum of all item totals).
    /// </summary>
    public decimal SubtotalAmount => Items.Sum(x => x.TotalPrice);
    
    /// <summary>
    /// Adds an item to the cart or increases quantity if it already exists.
    /// </summary>
    public void AddItem(CartItemModel item)
    {
        var existing = Items.FirstOrDefault(x => x.MenuItemId == item.MenuItemId);
        if (existing != null)
        {
            existing.Quantity += item.Quantity;
        }
        else
        {
            Items.Add(item);
        }
    }
    
    /// <summary>
    /// Removes an item from the cart.
    /// </summary>
    public void RemoveItem(Guid menuItemId)
    {
        Items.RemoveAll(x => x.MenuItemId == menuItemId);
    }
    
    /// <summary>
    /// Updates the quantity of an item in the cart.
    /// </summary>
    public void UpdateQuantity(Guid menuItemId, int quantity)
    {
        var item = Items.FirstOrDefault(x => x.MenuItemId == menuItemId);
        if (item != null)
        {
            if (quantity <= 0)
            {
                Items.Remove(item);
            }
            else
            {
                item.Quantity = quantity;
            }
        }
    }
    
    /// <summary>
    /// Clears all items from the cart.
    /// </summary>
    public void Clear()
    {
        Items.Clear();
    }
}
