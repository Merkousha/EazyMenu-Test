using System;
using System.Text;
using System.Text.Json;
using EazyMenu.Public.Models.Cart;
using EazyMenu.Public.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace EazyMenu.UnitTests.Presentation.Public.Services;

public sealed class SessionShoppingCartServiceTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessor = new();
    private readonly Mock<ISession> _session = new();
    private readonly Mock<HttpContext> _httpContext = new();

    public SessionShoppingCartServiceTests()
    {
        _httpContext.Setup(c => c.Session).Returns(_session.Object);
        _httpContextAccessor.Setup(a => a.HttpContext).Returns(_httpContext.Object);
    }

    [Fact]
    public void GetCart_WhenSessionIsNull_ReturnsEmptyCart()
    {
        // Arrange
        _httpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);
        var service = new SessionShoppingCartService(_httpContextAccessor.Object);

        // Act
        var cart = service.GetCart();

        // Assert
        Assert.NotNull(cart);
        Assert.Empty(cart.Items);
        Assert.Equal(0, cart.TotalItemCount);
        Assert.Equal(0m, cart.SubtotalAmount);
    }

    [Fact]
    public void GetCart_WhenNoCartInSession_ReturnsEmptyCart()
    {
        // Arrange
        _session.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]?>.IsAny!))
            .Returns(false);

        var service = new SessionShoppingCartService(_httpContextAccessor.Object);

        // Act
        var cart = service.GetCart();

        // Assert
        Assert.NotNull(cart);
        Assert.Empty(cart.Items);
    }

    [Fact]
    public void GetCart_WhenCartExistsInSession_ReturnsDeserializedCart()
    {
        // Arrange
        var menuId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var existingCart = new ShoppingCartModel { TenantId = tenantId, MenuId = menuId };
        existingCart.AddItem(new CartItemModel { MenuItemId = Guid.NewGuid(), DisplayName = "پیتزا", UnitPrice = 100000m, Quantity = 2 });

        var cartJson = JsonSerializer.Serialize(existingCart);
        SetupSessionWithCart(cartJson);

        var service = new SessionShoppingCartService(_httpContextAccessor.Object);

        // Act
        var cart = service.GetCart();

        // Assert
        Assert.NotNull(cart);
        Assert.Single(cart.Items);
        Assert.Equal(2, cart.TotalItemCount);
        Assert.Equal(200000m, cart.SubtotalAmount);
        Assert.Equal(tenantId, cart.TenantId);
        Assert.Equal(menuId, cart.MenuId);
    }

    [Fact]
    public void GetCart_WhenInvalidJson_ReturnsEmptyCart()
    {
        // Arrange
        SetupSessionWithCart("invalid json");

        var service = new SessionShoppingCartService(_httpContextAccessor.Object);

        // Act
        var cart = service.GetCart();

        // Assert
        Assert.NotNull(cart);
        Assert.Empty(cart.Items);
    }

    [Fact]
    public void AddItem_AddsItemToCartAndSavesToSession()
    {
        // Arrange
        var menuItemId = Guid.NewGuid();
        byte[]? savedData = null;

        _session.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]?>.IsAny!))
            .Returns(false);

        _session.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Callback<string, byte[]>((key, value) => savedData = value);

        var service = new SessionShoppingCartService(_httpContextAccessor.Object);

        // Act
        service.AddItem(menuItemId, "پیتزا مخصوص", 150000m, 3, "بدون پنیر");

        // Assert
        Assert.NotNull(savedData);
        var savedJson = Encoding.UTF8.GetString(savedData);
        var savedCart = JsonSerializer.Deserialize<ShoppingCartModel>(savedJson);
        
        Assert.NotNull(savedCart);
        Assert.Single(savedCart.Items);
        Assert.Equal(menuItemId, savedCart.Items[0].MenuItemId);
        Assert.Equal("پیتزا مخصوص", savedCart.Items[0].DisplayName);
        Assert.Equal(150000m, savedCart.Items[0].UnitPrice);
        Assert.Equal(3, savedCart.Items[0].Quantity);
        Assert.Equal("بدون پنیر", savedCart.Items[0].Note);
    }

    [Fact]
    public void AddItem_WhenItemAlreadyExists_IncreasesQuantity()
    {
        // Arrange
        var menuItemId = Guid.NewGuid();
        var existingCart = new ShoppingCartModel();
        existingCart.AddItem(new CartItemModel { MenuItemId = menuItemId, DisplayName = "پیتزا", UnitPrice = 100000m, Quantity = 2 });

        var cartJson = JsonSerializer.Serialize(existingCart);
        SetupSessionWithCart(cartJson);

        byte[]? savedData = null;
        _session.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Callback<string, byte[]>((key, value) => savedData = value);

        var service = new SessionShoppingCartService(_httpContextAccessor.Object);

        // Act
        service.AddItem(menuItemId, "پیتزا", 100000m, 3, null);

        // Assert
        Assert.NotNull(savedData);
        var savedJson = Encoding.UTF8.GetString(savedData);
        var savedCart = JsonSerializer.Deserialize<ShoppingCartModel>(savedJson);
        
        Assert.NotNull(savedCart);
        Assert.Single(savedCart.Items);
        Assert.Equal(5, savedCart.Items[0].Quantity); // 2 + 3
    }

    [Fact]
    public void RemoveItem_RemovesItemFromCartAndSavesToSession()
    {
        // Arrange
        var menuItemId = Guid.NewGuid();
        var existingCart = new ShoppingCartModel();
        existingCart.AddItem(new CartItemModel { MenuItemId = menuItemId, DisplayName = "پیتزا", UnitPrice = 100000m, Quantity = 2 });
        existingCart.AddItem(new CartItemModel { MenuItemId = Guid.NewGuid(), DisplayName = "نوشابه", UnitPrice = 15000m, Quantity = 1 });

        var cartJson = JsonSerializer.Serialize(existingCart);
        SetupSessionWithCart(cartJson);

        byte[]? savedData = null;
        _session.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Callback<string, byte[]>((key, value) => savedData = value);

        var service = new SessionShoppingCartService(_httpContextAccessor.Object);

        // Act
        service.RemoveItem(menuItemId);

        // Assert
        Assert.NotNull(savedData);
        var savedJson = Encoding.UTF8.GetString(savedData);
        var savedCart = JsonSerializer.Deserialize<ShoppingCartModel>(savedJson);
        
        Assert.NotNull(savedCart);
        Assert.Single(savedCart.Items);
        Assert.NotEqual(menuItemId, savedCart.Items[0].MenuItemId);
    }

    [Fact]
    public void RemoveItem_WhenItemDoesNotExist_DoesNotThrow()
    {
        // Arrange
        var existingCart = new ShoppingCartModel();
        existingCart.AddItem(new CartItemModel { MenuItemId = Guid.NewGuid(), DisplayName = "پیتزا", UnitPrice = 100000m, Quantity = 1 });

        var cartJson = JsonSerializer.Serialize(existingCart);
        SetupSessionWithCart(cartJson);

        var service = new SessionShoppingCartService(_httpContextAccessor.Object);

        // Act & Assert
        var exception = Record.Exception(() => service.RemoveItem(Guid.NewGuid()));
        Assert.Null(exception);
    }

    [Fact]
    public void UpdateQuantity_UpdatesItemQuantityAndSavesToSession()
    {
        // Arrange
        var menuItemId = Guid.NewGuid();
        var existingCart = new ShoppingCartModel();
        existingCart.AddItem(new CartItemModel { MenuItemId = menuItemId, DisplayName = "پیتزا", UnitPrice = 100000m, Quantity = 2 });

        var cartJson = JsonSerializer.Serialize(existingCart);
        SetupSessionWithCart(cartJson);

        byte[]? savedData = null;
        _session.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Callback<string, byte[]>((key, value) => savedData = value);

        var service = new SessionShoppingCartService(_httpContextAccessor.Object);

        // Act
        service.UpdateQuantity(menuItemId, 5);

        // Assert
        Assert.NotNull(savedData);
        var savedJson = Encoding.UTF8.GetString(savedData);
        var savedCart = JsonSerializer.Deserialize<ShoppingCartModel>(savedJson);
        
        Assert.NotNull(savedCart);
        Assert.Single(savedCart.Items);
        Assert.Equal(5, savedCart.Items[0].Quantity);
    }

    [Fact]
    public void UpdateQuantity_WhenItemDoesNotExist_DoesNotThrow()
    {
        // Arrange
        var existingCart = new ShoppingCartModel();
        var cartJson = JsonSerializer.Serialize(existingCart);
        SetupSessionWithCart(cartJson);

        var service = new SessionShoppingCartService(_httpContextAccessor.Object);

        // Act & Assert - should not throw, operation is silently ignored
        var exception = Record.Exception(() => 
            service.UpdateQuantity(Guid.NewGuid(), 5));
        Assert.Null(exception);
    }

    [Fact]
    public void ClearCart_RemovesCartFromSession()
    {
        // Arrange
        var existingCart = new ShoppingCartModel();
        existingCart.AddItem(new CartItemModel { MenuItemId = Guid.NewGuid(), DisplayName = "پیتزا", UnitPrice = 100000m, Quantity = 2 });

        var cartJson = JsonSerializer.Serialize(existingCart);
        SetupSessionWithCart(cartJson);

        var service = new SessionShoppingCartService(_httpContextAccessor.Object);

        // Act
        service.ClearCart();

        // Assert
        _session.Verify(s => s.Remove("EazyMenu_ShoppingCart"), Times.Once);
    }

    [Fact]
    public void ClearCart_WhenSessionIsNull_DoesNotThrow()
    {
        // Arrange
        _httpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);
        var service = new SessionShoppingCartService(_httpContextAccessor.Object);

        // Act & Assert
        var exception = Record.Exception(() => service.ClearCart());
        Assert.Null(exception);
    }

    [Fact]
    public void SetMenuContext_SetsMenuIdAndTenantIdAndSavesToSession()
    {
        // Arrange
        var menuId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        _session.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]?>.IsAny!))
            .Returns(false);

        byte[]? savedData = null;
        _session.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Callback<string, byte[]>((key, value) => savedData = value);

        var service = new SessionShoppingCartService(_httpContextAccessor.Object);

        // Act
        service.SetMenuContext(menuId, tenantId);

        // Assert
        Assert.NotNull(savedData);
        var savedJson = Encoding.UTF8.GetString(savedData);
        var savedCart = JsonSerializer.Deserialize<ShoppingCartModel>(savedJson);
        
        Assert.NotNull(savedCart);
        Assert.Equal(menuId, savedCart.MenuId);
        Assert.Equal(tenantId, savedCart.TenantId);
    }

    [Fact]
    public void SetMenuContext_WhenCartHasItems_PreservesItems()
    {
        // Arrange
        var existingCart = new ShoppingCartModel();
        existingCart.AddItem(new CartItemModel { MenuItemId = Guid.NewGuid(), DisplayName = "پیتزا", UnitPrice = 100000m, Quantity = 2 });

        var cartJson = JsonSerializer.Serialize(existingCart);
        SetupSessionWithCart(cartJson);

        var newMenuId = Guid.NewGuid();
        var newTenantId = Guid.NewGuid();

        byte[]? savedData = null;
        _session.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Callback<string, byte[]>((key, value) => savedData = value);

        var service = new SessionShoppingCartService(_httpContextAccessor.Object);

        // Act
        service.SetMenuContext(newMenuId, newTenantId);

        // Assert
        Assert.NotNull(savedData);
        var savedJson = Encoding.UTF8.GetString(savedData);
        var savedCart = JsonSerializer.Deserialize<ShoppingCartModel>(savedJson);
        
        Assert.NotNull(savedCart);
        Assert.Equal(newMenuId, savedCart.MenuId);
        Assert.Equal(newTenantId, savedCart.TenantId);
        Assert.Single(savedCart.Items); // Items preserved
    }

    private void SetupSessionWithCart(string cartJson)
    {
        var cartBytes = Encoding.UTF8.GetBytes(cartJson);
        _session.Setup(s => s.TryGetValue("EazyMenu_ShoppingCart", out It.Ref<byte[]?>.IsAny!))
            .Callback(new TryGetValueCallback((string key, out byte[]? value) =>
            {
                value = cartBytes;
            }))
            .Returns(true);
    }

    private delegate void TryGetValueCallback(string key, out byte[]? value);
}
