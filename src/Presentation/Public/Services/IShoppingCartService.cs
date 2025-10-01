using System;
using EazyMenu.Public.Models.Cart;

namespace EazyMenu.Public.Services;

/// <summary>
/// Service for managing customer shopping cart during a session.
/// </summary>
public interface IShoppingCartService
{
    /// <summary>
    /// Gets the current shopping cart for the session.
    /// </summary>
    ShoppingCartModel GetCart();
    
    /// <summary>
    /// Adds an item to the cart or increases quantity if it already exists.
    /// </summary>
    void AddItem(Guid menuItemId, string displayName, decimal unitPrice, int quantity, string? note = null);
    
    /// <summary>
    /// Removes an item from the cart.
    /// </summary>
    void RemoveItem(Guid menuItemId);
    
    /// <summary>
    /// Updates the quantity of an item in the cart.
    /// </summary>
    void UpdateQuantity(Guid menuItemId, int quantity);
    
    /// <summary>
    /// Clears all items from the cart.
    /// </summary>
    void ClearCart();
    
    /// <summary>
    /// Sets the menu context for the cart (MenuId and TenantId).
    /// </summary>
    void SetMenuContext(Guid menuId, Guid tenantId);
}
