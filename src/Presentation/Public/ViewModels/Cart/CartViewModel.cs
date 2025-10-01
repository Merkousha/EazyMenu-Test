using System.Collections.Generic;
using EazyMenu.Public.Models.Cart;

namespace EazyMenu.Public.ViewModels.Cart;

/// <summary>
/// ViewModel for the shopping cart page.
/// </summary>
public sealed class CartViewModel
{
    public List<CartItemModel> Items { get; set; } = new();
    public int TotalItemCount { get; set; }
    public decimal SubtotalAmount { get; set; }
    public bool IsEmpty => Items.Count == 0;
}
