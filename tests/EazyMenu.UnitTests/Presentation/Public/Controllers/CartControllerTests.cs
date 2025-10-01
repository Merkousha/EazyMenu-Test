using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Features.Orders.Common;
using EazyMenu.Application.Features.Orders.GetOrderDetails;
using EazyMenu.Application.Features.Orders.PlaceOrder;
using EazyMenu.Domain.ValueObjects;
using EazyMenu.Public.Controllers;
using EazyMenu.Public.Models.Cart;
using EazyMenu.Public.Services;
using EazyMenu.Public.ViewModels.Cart;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace EazyMenu.UnitTests.Presentation.Public.Controllers;

public sealed class CartControllerTests
{
    private readonly Mock<IShoppingCartService> _cartService = new();
    private readonly Mock<ICommandHandler<PlaceOrderCommand, OrderId>> _placeOrderHandler = new();
    private readonly Mock<IQueryHandler<GetOrderDetailsQuery, OrderDetailsDto>> _getOrderDetailsHandler = new();

    [Fact]
    public void Index_ReturnsViewWithCartViewModel()
    {
        // Arrange
        var cart = new ShoppingCartModel();
        cart.AddItem(new CartItemModel
        {
            MenuItemId = Guid.NewGuid(),
            DisplayName = "پیتزا",
            UnitPrice = 100000m,
            Quantity = 2,
            Note = null
        });

        _cartService.Setup(s => s.GetCart()).Returns(cart);

        var controller = CreateController();

        // Act
        var result = controller.Index();

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<CartViewModel>(view.Model);
        Assert.Equal(2, model.TotalItemCount);
        Assert.Equal(200000m, model.SubtotalAmount);
        Assert.Single(model.Items);
    }

    [Fact]
    public void AddToCart_WithValidData_AddsItemAndRedirects()
    {
        // Arrange
        var menuItemId = Guid.NewGuid();
        var menuId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var controller = CreateController();

        // Act
        var result = controller.AddToCart(menuItemId, "پیتزا", 100000m, 2, null, menuId, tenantId);

        // Assert
        _cartService.Verify(s => s.SetMenuContext(menuId, tenantId), Times.Once);
        _cartService.Verify(s => s.AddItem(menuItemId, "پیتزا", 100000m, 2, null), Times.Once);
        
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("محصول به سبد خرید اضافه شد.", controller.TempData["Success"]);
    }

    [Fact]
    public void AddToCart_WithoutMenuContext_AddsItemWithoutSettingContext()
    {
        // Arrange
        var menuItemId = Guid.NewGuid();
        var controller = CreateController();

        // Act
        var result = controller.AddToCart(menuItemId, "نوشابه", 15000m, 1, null, null, null);

        // Assert
        _cartService.Verify(s => s.SetMenuContext(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        _cartService.Verify(s => s.AddItem(menuItemId, "نوشابه", 15000m, 1, null), Times.Once);
        
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }

    [Fact]
    public void AddToCart_WhenExceptionOccurs_SetsTempDataErrorAndRedirects()
    {
        // Arrange
        var menuItemId = Guid.NewGuid();
        _cartService.Setup(s => s.AddItem(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<string?>()))
            .Throws(new InvalidOperationException("Test exception"));

        var controller = CreateController();

        // Act
        var result = controller.AddToCart(menuItemId, "پیتزا", 100000m, 1, null, null, null);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("خطا در افزودن محصول به سبد خرید.", controller.TempData["Error"]);
    }

    [Fact]
    public void RemoveFromCart_WithValidId_RemovesItemAndRedirects()
    {
        // Arrange
        var menuItemId = Guid.NewGuid();
        var controller = CreateController();

        // Act
        var result = controller.RemoveFromCart(menuItemId);

        // Assert
        _cartService.Verify(s => s.RemoveItem(menuItemId), Times.Once);
        
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("محصول از سبد خرید حذف شد.", controller.TempData["Success"]);
    }

    [Fact]
    public void RemoveFromCart_WhenExceptionOccurs_SetsTempDataErrorAndRedirects()
    {
        // Arrange
        var menuItemId = Guid.NewGuid();
        _cartService.Setup(s => s.RemoveItem(It.IsAny<Guid>()))
            .Throws(new InvalidOperationException("Test exception"));

        var controller = CreateController();

        // Act
        var result = controller.RemoveFromCart(menuItemId);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("خطا در حذف محصول.", controller.TempData["Error"]);
    }

    [Fact]
    public void UpdateQuantity_WithValidData_UpdatesQuantityAndRedirects()
    {
        // Arrange
        var menuItemId = Guid.NewGuid();
        var controller = CreateController();

        // Act
        var result = controller.UpdateQuantity(menuItemId, 5);

        // Assert
        _cartService.Verify(s => s.UpdateQuantity(menuItemId, 5), Times.Once);
        
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("تعداد محصول به‌روز شد.", controller.TempData["Success"]);
    }

    [Fact]
    public void UpdateQuantity_WhenExceptionOccurs_SetsTempDataErrorAndRedirects()
    {
        // Arrange
        var menuItemId = Guid.NewGuid();
        _cartService.Setup(s => s.UpdateQuantity(It.IsAny<Guid>(), It.IsAny<int>()))
            .Throws(new ArgumentException("Invalid quantity"));

        var controller = CreateController();

        // Act
        var result = controller.UpdateQuantity(menuItemId, -1);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("خطا در به‌روزرسانی تعداد.", controller.TempData["Error"]);
    }

    [Fact]
    public void ClearCart_ClearsCartAndRedirects()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.ClearCart();

        // Assert
        _cartService.Verify(s => s.ClearCart(), Times.Once);
        
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("سبد خرید خالی شد.", controller.TempData["Success"]);
    }

    [Fact]
    public void ClearCart_WhenExceptionOccurs_SetsTempDataErrorAndRedirects()
    {
        // Arrange
        _cartService.Setup(s => s.ClearCart())
            .Throws(new InvalidOperationException("Test exception"));

        var controller = CreateController();

        // Act
        var result = controller.ClearCart();

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("خطا در خالی کردن سبد خرید.", controller.TempData["Error"]);
    }

    [Fact]
    public void Checkout_Get_WithItems_ReturnsViewWithCheckoutViewModel()
    {
        // Arrange
        var cart = new ShoppingCartModel();
        cart.AddItem(new CartItemModel { MenuItemId = Guid.NewGuid(), DisplayName = "پیتزا", UnitPrice = 100000m, Quantity = 2 });

        _cartService.Setup(s => s.GetCart()).Returns(cart);

        var controller = CreateController();

        // Act
        var result = controller.Checkout();

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<CheckoutViewModel>(view.Model);
        Assert.Single(model.Items);
        Assert.Equal(200000m, model.SubtotalAmount);
        Assert.Equal(0m, model.DeliveryFee);
        Assert.Equal(0m, model.TaxAmount);
    }

    [Fact]
    public void Checkout_Get_WithEmptyCart_RedirectsToIndexWithError()
    {
        // Arrange
        var cart = new ShoppingCartModel();
        _cartService.Setup(s => s.GetCart()).Returns(cart);

        var controller = CreateController();

        // Act
        var result = controller.Checkout();

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("سبد خرید شما خالی است.", controller.TempData["Error"]);
    }

    [Fact]
    public async Task Checkout_Post_WithValidModel_PlacesOrderAndRedirects()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var menuId = Guid.NewGuid();
        var orderId = OrderId.New();

        var cart = new ShoppingCartModel { TenantId = tenantId, MenuId = menuId };
        cart.AddItem(new CartItemModel { MenuItemId = Guid.NewGuid(), DisplayName = "پیتزا", UnitPrice = 100000m, Quantity = 2 });

        _cartService.Setup(s => s.GetCart()).Returns(cart);
        _placeOrderHandler
            .Setup(h => h.HandleAsync(It.IsAny<PlaceOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderId);

        var controller = CreateController();
        var model = new CheckoutViewModel
        {
            CustomerName = "علی احمدی",
            CustomerPhone = "09121234567",
            FulfillmentMethod = "Pickup",
            CustomerNote = "لطفاً زودتر آماده کنید"
        };

        // Act
        var result = await controller.Checkout(model);

        // Assert
        _placeOrderHandler.Verify(h => h.HandleAsync(
            It.Is<PlaceOrderCommand>(cmd => 
                cmd.TenantId == tenantId &&
                cmd.MenuId == menuId &&
                cmd.CustomerName == "علی احمدی" &&
                cmd.CustomerPhone == "09121234567" &&
                cmd.FulfillmentMethod == "Pickup" &&
                cmd.DeliveryFee == 0m &&
                cmd.Items.Count == 1),
            It.IsAny<CancellationToken>()), Times.Once);

        _cartService.Verify(s => s.ClearCart(), Times.Once);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("OrderConfirmation", redirect.ActionName);
        Assert.NotNull(redirect.RouteValues);
        Assert.True(redirect.RouteValues.ContainsKey("orderId"));
        Assert.Equal(orderId.Value, (Guid)redirect.RouteValues["orderId"]!);
        Assert.Equal("سفارش شما با موفقیت ثبت شد!", controller.TempData["Success"]);
    }

    [Fact]
    public async Task Checkout_Post_WithDeliveryMethod_CalculatesDeliveryFee()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var menuId = Guid.NewGuid();
        var orderId = OrderId.New();

        var cart = new ShoppingCartModel { TenantId = tenantId, MenuId = menuId };
        cart.AddItem(new CartItemModel { MenuItemId = Guid.NewGuid(), DisplayName = "پیتزا", UnitPrice = 100000m, Quantity = 1 });

        _cartService.Setup(s => s.GetCart()).Returns(cart);
        _placeOrderHandler
            .Setup(h => h.HandleAsync(It.IsAny<PlaceOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderId);

        var controller = CreateController();
        var model = new CheckoutViewModel
        {
            CustomerName = "علی احمدی",
            CustomerPhone = "09121234567",
            FulfillmentMethod = "Delivery",
            CustomerNote = null
        };

        // Act
        var result = await controller.Checkout(model);

        // Assert
        _placeOrderHandler.Verify(h => h.HandleAsync(
            It.Is<PlaceOrderCommand>(cmd => cmd.DeliveryFee == 20000m),
            It.IsAny<CancellationToken>()), Times.Once);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("OrderConfirmation", redirect.ActionName);
    }

    [Fact]
    public async Task Checkout_Post_WithInvalidModel_ReturnsViewWithErrors()
    {
        // Arrange
        var cart = new ShoppingCartModel();
        cart.AddItem(new CartItemModel { MenuItemId = Guid.NewGuid(), DisplayName = "پیتزا", UnitPrice = 100000m, Quantity = 1 });

        _cartService.Setup(s => s.GetCart()).Returns(cart);

        var controller = CreateController();
        controller.ModelState.AddModelError("CustomerName", "نام الزامی است");

        var model = new CheckoutViewModel();

        // Act
        var result = await controller.Checkout(model);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var returnedModel = Assert.IsType<CheckoutViewModel>(view.Model);
        Assert.Single(returnedModel.Items);
        Assert.Equal(100000m, returnedModel.SubtotalAmount);
        
        _placeOrderHandler.Verify(h => h.HandleAsync(
            It.IsAny<PlaceOrderCommand>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Checkout_Post_WithEmptyCart_RedirectsToIndexWithError()
    {
        // Arrange
        var cart = new ShoppingCartModel();
        _cartService.Setup(s => s.GetCart()).Returns(cart);

        var controller = CreateController();
        var model = new CheckoutViewModel
        {
            CustomerName = "علی احمدی",
            CustomerPhone = "09121234567",
            FulfillmentMethod = "Pickup"
        };

        // Act
        var result = await controller.Checkout(model);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("سبد خرید شما خالی است.", controller.TempData["Error"]);
        
        _placeOrderHandler.Verify(h => h.HandleAsync(
            It.IsAny<PlaceOrderCommand>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Checkout_Post_WhenHandlerThrows_ReturnsViewWithError()
    {
        // Arrange
        var cart = new ShoppingCartModel();
        cart.AddItem(new CartItemModel { MenuItemId = Guid.NewGuid(), DisplayName = "پیتزا", UnitPrice = 100000m, Quantity = 1 });

        _cartService.Setup(s => s.GetCart()).Returns(cart);
        _placeOrderHandler
            .Setup(h => h.HandleAsync(It.IsAny<PlaceOrderCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        var controller = CreateController();
        var model = new CheckoutViewModel
        {
            CustomerName = "علی احمدی",
            CustomerPhone = "09121234567",
            FulfillmentMethod = "Pickup"
        };

        // Act
        var result = await controller.Checkout(model);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var returnedModel = Assert.IsType<CheckoutViewModel>(view.Model);
        Assert.Single(returnedModel.Items);
        Assert.Equal("خطا در ثبت سفارش. لطفاً دوباره تلاش کنید.", controller.TempData["Error"]);
        
        _cartService.Verify(s => s.ClearCart(), Times.Never);
    }

    [Fact]
    public async Task OrderConfirmation_ReturnsViewWithViewModel()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var cart = new ShoppingCartModel { TenantId = tenantId, MenuId = Guid.NewGuid() };
        _cartService.Setup(s => s.GetCart()).Returns(cart);
        
        var orderDetails = new OrderDetailsDto(
            OrderId: orderId,
            TenantId: tenantId,
            MenuId: Guid.NewGuid(),
            OrderNumber: "ORD-20251001-0001",
            Status: OrderStatus.Pending,
            FulfillmentMethod: FulfillmentMethod.Pickup,
            CustomerName: "علی احمدی",
            CustomerPhone: "09121234567",
            CustomerNote: null,
            SubtotalAmount: 100000m,
            TaxAmount: 0m,
            DeliveryFee: 0m,
            TotalAmount: 100000m,
            CreatedAtUtc: DateTime.UtcNow,
            ConfirmedAtUtc: null,
            CompletedAtUtc: null,
            CancelledAtUtc: null,
            CancellationReason: null,
            Items: new List<OrderItemDto>()
        );
        
        _getOrderDetailsHandler
            .Setup(h => h.HandleAsync(It.IsAny<GetOrderDetailsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderDetails);
        
        var controller = CreateController();

        // Act
        var result = await controller.OrderConfirmation(orderId);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<OrderConfirmationViewModel>(view.Model);
        Assert.NotNull(model.OrderNumber);
        Assert.NotNull(model.CustomerName);
    }

    [Fact]
    public async Task OrderConfirmation_WithValidOrder_FetchesRealOrderDetails()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var orderDetails = new OrderDetailsDto(
            OrderId: orderId,
            TenantId: tenantId,
            MenuId: Guid.NewGuid(),
            OrderNumber: "ORD-20251001-0001",
            Status: OrderStatus.Pending,
            FulfillmentMethod: FulfillmentMethod.Pickup,
            CustomerName: "علی احمدی",
            CustomerPhone: "09121234567",
            CustomerNote: null,
            SubtotalAmount: 200000m,
            TaxAmount: 0m,
            DeliveryFee: 0m,
            TotalAmount: 200000m,
            CreatedAtUtc: DateTime.UtcNow,
            ConfirmedAtUtc: null,
            CompletedAtUtc: null,
            CancelledAtUtc: null,
            CancellationReason: null,
            Items: new List<OrderItemDto>()
        );

        var cart = new ShoppingCartModel { TenantId = tenantId, MenuId = Guid.NewGuid() };
        _cartService.Setup(s => s.GetCart()).Returns(cart);
        _getOrderDetailsHandler
            .Setup(h => h.HandleAsync(It.IsAny<GetOrderDetailsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderDetails);

        var controller = CreateController();

        // Act
        var result = await controller.OrderConfirmation(orderId);

        // Assert
        _getOrderDetailsHandler.Verify(h => h.HandleAsync(
            It.Is<GetOrderDetailsQuery>(q => q.TenantId == tenantId && q.OrderId == orderId),
            It.IsAny<CancellationToken>()), Times.Once);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<OrderConfirmationViewModel>(view.Model);
        Assert.Equal("ORD-20251001-0001", model.OrderNumber);
        Assert.Equal("علی احمدی", model.CustomerName);
        Assert.Equal("09121234567", model.CustomerPhone);
        Assert.Equal("Pickup", model.FulfillmentMethod);
        Assert.Equal(200000m, model.TotalAmount);
        Assert.Equal("15-20 دقیقه", model.EstimatedReadyTime);
    }

    [Fact]
    public async Task OrderConfirmation_WithDeliveryOrder_ShowsLongerEstimatedTime()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var orderDetails = new OrderDetailsDto(
            OrderId: orderId,
            TenantId: tenantId,
            MenuId: Guid.NewGuid(),
            OrderNumber: "ORD-20251001-0002",
            Status: OrderStatus.Pending,
            FulfillmentMethod: FulfillmentMethod.Delivery,
            CustomerName: "علی احمدی",
            CustomerPhone: "09121234567",
            CustomerNote: null,
            SubtotalAmount: 200000m,
            TaxAmount: 0m,
            DeliveryFee: 20000m,
            TotalAmount: 220000m,
            CreatedAtUtc: DateTime.UtcNow,
            ConfirmedAtUtc: null,
            CompletedAtUtc: null,
            CancelledAtUtc: null,
            CancellationReason: null,
            Items: new List<OrderItemDto>()
        );

        var cart = new ShoppingCartModel { TenantId = tenantId, MenuId = Guid.NewGuid() };
        _cartService.Setup(s => s.GetCart()).Returns(cart);
        _getOrderDetailsHandler
            .Setup(h => h.HandleAsync(It.IsAny<GetOrderDetailsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderDetails);

        var controller = CreateController();

        // Act
        var result = await controller.OrderConfirmation(orderId);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<OrderConfirmationViewModel>(view.Model);
        Assert.Equal("30-45 دقیقه", model.EstimatedReadyTime);
    }

    [Fact]
    public async Task OrderConfirmation_WhenNoTenantContext_RedirectsToHome()
    {
        // Arrange
        var cart = new ShoppingCartModel(); // Empty tenant context
        _cartService.Setup(s => s.GetCart()).Returns(cart);

        var controller = CreateController();

        // Act
        var result = await controller.OrderConfirmation(Guid.NewGuid());

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);
    }

    [Fact]
    public async Task OrderConfirmation_WhenOrderNotFound_RedirectsToHome()
    {
        // Arrange
        var cart = new ShoppingCartModel { TenantId = Guid.NewGuid(), MenuId = Guid.NewGuid() };
        _cartService.Setup(s => s.GetCart()).Returns(cart);
        _getOrderDetailsHandler
            .Setup(h => h.HandleAsync(It.IsAny<GetOrderDetailsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderDetailsDto)null!);

        var controller = CreateController();

        // Act
        var result = await controller.OrderConfirmation(Guid.NewGuid());

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);
    }

    [Fact]
    public async Task OrderConfirmation_WhenQueryThrows_ShowsFallbackConfirmation()
    {
        // Arrange
        var cart = new ShoppingCartModel { TenantId = Guid.NewGuid(), MenuId = Guid.NewGuid() };
        _cartService.Setup(s => s.GetCart()).Returns(cart);
        _getOrderDetailsHandler
            .Setup(h => h.HandleAsync(It.IsAny<GetOrderDetailsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        var controller = CreateController();

        // Act
        var result = await controller.OrderConfirmation(Guid.NewGuid());

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<OrderConfirmationViewModel>(view.Model);
        Assert.NotNull(model.OrderNumber);
        Assert.Equal("مشتری", model.CustomerName);
    }

    private CartController CreateController()
    {
        var controller = new CartController(
            _cartService.Object,
            _placeOrderHandler.Object,
            _getOrderDetailsHandler.Object,
            NullLogger<CartController>.Instance)
        {
            TempData = new TempDataDictionary(
                new Microsoft.AspNetCore.Http.DefaultHttpContext(),
                Mock.Of<ITempDataProvider>())
        };

        return controller;
    }
}
