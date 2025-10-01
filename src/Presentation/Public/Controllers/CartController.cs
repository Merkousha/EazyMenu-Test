using System;
using System.Linq;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Features.Orders.Common;
using EazyMenu.Application.Features.Orders.GetOrderDetails;
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
    private readonly IQueryHandler<GetOrderDetailsQuery, OrderDetailsDto> _getOrderDetailsHandler;
    private readonly ILogger<CartController> _logger;

    public CartController(
        IShoppingCartService cartService,
        ICommandHandler<PlaceOrderCommand, OrderId> placeOrderHandler,
        IQueryHandler<GetOrderDetailsQuery, OrderDetailsDto> getOrderDetailsHandler,
        ILogger<CartController> logger)
    {
        _cartService = cartService;
        _placeOrderHandler = placeOrderHandler;
        _getOrderDetailsHandler = getOrderDetailsHandler;
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
            return RedirectToAction("OrderConfirmation", new { orderId = orderId.Value });
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
    public async Task<IActionResult> OrderConfirmation(Guid orderId)
    {
        try
        {
            // Get cart to retrieve tenant context
            var cart = _cartService.GetCart();
            if (cart.TenantId == Guid.Empty)
            {
                _logger.LogWarning("No tenant context found for order confirmation");
                return RedirectToAction("Index", "Home");
            }

            // Fetch order details
            var query = new GetOrderDetailsQuery(cart.TenantId, orderId);
            var orderDetails = await _getOrderDetailsHandler.HandleAsync(query);

            if (orderDetails == null)
            {
                _logger.LogWarning("Order {OrderId} not found", orderId);
                return RedirectToAction("Index", "Home");
            }

            // Map to view model
            var viewModel = new OrderConfirmationViewModel
            {
                OrderNumber = orderDetails.OrderNumber,
                CustomerName = orderDetails.CustomerName,
                CustomerPhone = orderDetails.CustomerPhone,
                FulfillmentMethod = orderDetails.FulfillmentMethod.ToString(),
                TotalAmount = orderDetails.TotalAmount,
                CreatedAt = orderDetails.CreatedAtUtc,
                EstimatedReadyTime = CalculateEstimatedReadyTime(orderDetails.FulfillmentMethod)
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching order confirmation details for order {OrderId}", orderId);
            
            // Fallback to simple confirmation
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

    private static string CalculateEstimatedReadyTime(FulfillmentMethod method)
    {
        return method switch
        {
            FulfillmentMethod.Delivery => "30-45 دقیقه",
            FulfillmentMethod.Pickup => "15-20 دقیقه",
            FulfillmentMethod.DineIn => "20-30 دقیقه",
            _ => "15-20 دقیقه"
        };
    }
}
