using System;
using System.Linq;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Features.Orders.PlaceOrder;
using EazyMenu.Domain.ValueObjects;
using EazyMenu.Public.Services;
using EazyMenu.Public.ViewModels.Cart;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EazyMenu.Public.Controllers;

public sealed class CartController : Controller
{
    private readonly IShoppingCartService _cartService;
    private readonly ICommandHandler<PlaceOrderCommand, OrderId> _placeOrderHandler;
    private readonly ILogger<CartController> _logger;

    public CartController(
        IShoppingCartService cartService,
        ICommandHandler<PlaceOrderCommand, OrderId> placeOrderHandler,
        ILogger<CartController> logger)
    {
        _cartService = cartService;
        _placeOrderHandler = placeOrderHandler;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var cart = _cartService.GetCart();
        var viewModel = new CartViewModel
        {
            Items = cart.Items,
            TotalItemCount = cart.TotalItemCount,
            SubtotalAmount = cart.SubtotalAmount
        };
        
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddToCart(Guid menuItemId, string displayName, decimal unitPrice, int quantity = 1, string? note = null, Guid? menuId = null, Guid? tenantId = null)
    {
        try
        {
            // Set menu context if provided
            if (menuId.HasValue && tenantId.HasValue)
            {
                _cartService.SetMenuContext(menuId.Value, tenantId.Value);
            }
            
            _cartService.AddItem(menuItemId, displayName, unitPrice, quantity, note);
            TempData["Success"] = "محصول به سبد خرید اضافه شد.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در افزودن محصول به سبد خرید");
            TempData["Error"] = "خطا در افزودن محصول به سبد خرید.";
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RemoveFromCart(Guid menuItemId)
    {
        try
        {
            _cartService.RemoveItem(menuItemId);
            TempData["Success"] = "محصول از سبد خرید حذف شد.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در حذف محصول از سبد خرید");
            TempData["Error"] = "خطا در حذف محصول.";
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateQuantity(Guid menuItemId, int quantity)
    {
        try
        {
            _cartService.UpdateQuantity(menuItemId, quantity);
            TempData["Success"] = "تعداد محصول به‌روز شد.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در به‌روزرسانی تعداد");
            TempData["Error"] = "خطا در به‌روزرسانی تعداد.";
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ClearCart()
    {
        try
        {
            _cartService.ClearCart();
            TempData["Success"] = "سبد خرید خالی شد.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در خالی کردن سبد خرید");
            TempData["Error"] = "خطا در خالی کردن سبد خرید.";
        }

        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Checkout()
    {
        var cart = _cartService.GetCart();
        
        if (cart.Items.Count == 0)
        {
            TempData["Error"] = "سبد خرید شما خالی است.";
            return RedirectToAction("Index");
        }

        var viewModel = new CheckoutViewModel
        {
            Items = cart.Items,
            SubtotalAmount = cart.SubtotalAmount,
            DeliveryFee = 0, // Will be calculated based on fulfillment method
            TaxAmount = 0 // Will be calculated if needed
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var cart = _cartService.GetCart();
            model.Items = cart.Items;
            model.SubtotalAmount = cart.SubtotalAmount;
            return View(model);
        }

        try
        {
            var cart = _cartService.GetCart();
            
            if (cart.Items.Count == 0)
            {
                TempData["Error"] = "سبد خرید شما خالی است.";
                return RedirectToAction("Index");
            }

            // Calculate delivery fee based on fulfillment method
            var deliveryFee = model.FulfillmentMethod == "Delivery" ? 20000m : 0m;

            // Create order command
            var command = new PlaceOrderCommand(
                TenantId: cart.TenantId,
                MenuId: cart.MenuId,
                FulfillmentMethod: model.FulfillmentMethod,
                CustomerName: model.CustomerName,
                CustomerPhone: model.CustomerPhone,
                CustomerNote: model.CustomerNote,
                DeliveryFee: deliveryFee,
                TaxAmount: 0m,
                Items: cart.Items.Select(item => new OrderItemInput(
                    MenuItemId: item.MenuItemId,
                    DisplayName: item.DisplayName,
                    UnitPrice: item.UnitPrice,
                    Quantity: item.Quantity,
                    Note: item.Note
                )).ToList()
            );

            var orderId = await _placeOrderHandler.HandleAsync(command);
            
            // Clear cart after successful order
            _cartService.ClearCart();
            
            TempData["Success"] = "سفارش شما با موفقیت ثبت شد!";
            return RedirectToAction("OrderConfirmation", new { orderId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در ثبت سفارش");
            TempData["Error"] = "خطا در ثبت سفارش. لطفاً دوباره تلاش کنید.";
            
            var cart = _cartService.GetCart();
            model.Items = cart.Items;
            model.SubtotalAmount = cart.SubtotalAmount;
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult OrderConfirmation(Guid orderId)
    {
        // TODO: Fetch order details from GetOrderDetailsQuery
        // For now, show a simple confirmation message
        
        var viewModel = new OrderConfirmationViewModel
        {
            OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-XXXX",
            CustomerName = "مشتری",
            CustomerPhone = "09XX-XXX-XXXX",
            FulfillmentMethod = "حضوری",
            TotalAmount = 0,
            CreatedAt = DateTime.UtcNow,
            EstimatedReadyTime = "15-20 دقیقه"
        };

        return View(viewModel);
    }
}
