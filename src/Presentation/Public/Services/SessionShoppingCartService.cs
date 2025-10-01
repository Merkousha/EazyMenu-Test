using System;
using System.Text.Json;
using EazyMenu.Public.Models.Cart;
using Microsoft.AspNetCore.Http;

namespace EazyMenu.Public.Services;

/// <summary>
/// Session-based implementation of shopping cart service.
/// </summary>
public sealed class SessionShoppingCartService : IShoppingCartService
{
    private const string CartSessionKey = "EazyMenu_ShoppingCart";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SessionShoppingCartService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public ShoppingCartModel GetCart()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null)
            return new ShoppingCartModel();

        var cartJson = session.GetString(CartSessionKey);
        if (string.IsNullOrEmpty(cartJson))
            return new ShoppingCartModel();

        try
        {
            return JsonSerializer.Deserialize<ShoppingCartModel>(cartJson) ?? new ShoppingCartModel();
        }
        catch
        {
            return new ShoppingCartModel();
        }
    }

    public void AddItem(Guid menuItemId, string displayName, decimal unitPrice, int quantity, string? note = null)
    {
        var cart = GetCart();
        cart.AddItem(new CartItemModel
        {
            MenuItemId = menuItemId,
            DisplayName = displayName,
            UnitPrice = unitPrice,
            Quantity = quantity,
            Note = note
        });
        SaveCart(cart);
    }

    public void RemoveItem(Guid menuItemId)
    {
        var cart = GetCart();
        cart.RemoveItem(menuItemId);
        SaveCart(cart);
    }

    public void UpdateQuantity(Guid menuItemId, int quantity)
    {
        var cart = GetCart();
        cart.UpdateQuantity(menuItemId, quantity);
        SaveCart(cart);
    }

    public void ClearCart()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        session?.Remove(CartSessionKey);
    }

    public void SetMenuContext(Guid menuId, Guid tenantId)
    {
        var cart = GetCart();
        cart.MenuId = menuId;
        cart.TenantId = tenantId;
        SaveCart(cart);
    }

    private void SaveCart(ShoppingCartModel cart)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session != null)
        {
            var cartJson = JsonSerializer.Serialize(cart);
            session.SetString(CartSessionKey, cartJson);
        }
    }
}
