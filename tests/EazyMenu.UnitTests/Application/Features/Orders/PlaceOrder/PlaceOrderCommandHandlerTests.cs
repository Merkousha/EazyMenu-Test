using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Common.Interfaces.Notifications;
using EazyMenu.Application.Common.Interfaces.Orders;
using EazyMenu.Application.Common.Notifications;
using EazyMenu.Application.Features.Orders.PlaceOrder;
using EazyMenu.Domain.Aggregates.Orders;
using EazyMenu.Domain.Aggregates.Tenants;
using EazyMenu.Domain.ValueObjects;
using Moq;
using Xunit;

namespace EazyMenu.UnitTests.Application.Features.Orders.PlaceOrder;

public class PlaceOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _mockRepository = new();
    private readonly Mock<IOrderNumberGenerator> _mockNumberGenerator = new();
    private readonly Mock<IOrderRealtimeNotifier> _mockRealtimeNotifier = new();
    private readonly Mock<ISmsSender> _mockSmsSender = new();
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
    private readonly PlaceOrderCommandHandler _handler;

    public PlaceOrderCommandHandlerTests()
    {
        _handler = new PlaceOrderCommandHandler(
            _mockRepository.Object,
            _mockNumberGenerator.Object,
            _mockRealtimeNotifier.Object,
            _mockSmsSender.Object,
            _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_CreatesOrderAndSendsSms()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var menuId = Guid.NewGuid();
        var orderNumber = "ORD-123";
        
        var command = new PlaceOrderCommand(
            TenantId: tenantId,
            MenuId: menuId,
            FulfillmentMethod: "Pickup",
            CustomerName: "علی رضایی",
            CustomerPhone: "09123456789",
            CustomerNote: "بدون سس",
            DeliveryFee: 0m,
            TaxAmount: 5000m,
            Items: new List<OrderItemInput>
            {
                new(Guid.NewGuid(), "پیتزا پپرونی", 150000m, 2, null)
            });

        _mockNumberGenerator
            .Setup(g => g.GenerateAsync(It.IsAny<TenantId>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderNumber);

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockRealtimeNotifier
            .Setup(n => n.PublishOrderChangedAsync(It.IsAny<OrderRealtimeNotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockSmsSender
            .Setup(s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SmsSendContext>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.IsType<OrderId>(result);

        _mockRepository.Verify(r => r.AddAsync(
            It.Is<Order>(o => 
                o.TenantId.Value == tenantId && 
                o.MenuId == menuId &&
                o.OrderNumber == orderNumber &&
                o.CustomerName == "علی رضایی" &&
                o.CustomerPhone.Value == "09123456789"),
            It.IsAny<CancellationToken>()), Times.Once);

        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        _mockRealtimeNotifier.Verify(n => n.PublishOrderChangedAsync(
            It.Is<OrderRealtimeNotification>(notif =>
                notif.TenantId == tenantId &&
                notif.OrderNumber == orderNumber &&
                notif.ChangeType == "order-created" &&
                notif.Status == "Pending"),
            It.IsAny<CancellationToken>()), Times.Once);

        _mockSmsSender.Verify(s => s.SendAsync(
            "09123456789",
            It.Is<string>(msg => msg.Contains(orderNumber) && msg.Contains("سفارش شما")),
            It.Is<SmsSendContext>(ctx => ctx.TenantId == tenantId && ctx.SubscriptionPlan == SubscriptionPlan.Starter),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_SmsFailure_DoesNotPreventOrderCreation()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var menuId = Guid.NewGuid();
        var orderNumber = "ORD-124";
        
        var command = new PlaceOrderCommand(
            TenantId: tenantId,
            MenuId: menuId,
            FulfillmentMethod: "Delivery",
            CustomerName: "سارا احمدی",
            CustomerPhone: "09987654321",
            CustomerNote: null,
            DeliveryFee: 15000m,
            TaxAmount: 3000m,
            Items: new List<OrderItemInput>
            {
                new(Guid.NewGuid(), "برگر کلاسیک", 80000m, 1, "خوب پخته")
            });

        _mockNumberGenerator
            .Setup(g => g.GenerateAsync(It.IsAny<TenantId>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderNumber);

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockRealtimeNotifier
            .Setup(n => n.PublishOrderChangedAsync(It.IsAny<OrderRealtimeNotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Simulate SMS failure
        _mockSmsSender
            .Setup(s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SmsSendContext>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("SMS service unavailable"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert - Order should still be created despite SMS failure
        Assert.IsType<OrderId>(result);

        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockRealtimeNotifier.Verify(n => n.PublishOrderChangedAsync(It.IsAny<OrderRealtimeNotification>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockSmsSender.Verify(s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SmsSendContext>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_InvalidTenantId_ThrowsBusinessRuleViolation()
    {
        // Arrange
        var command = new PlaceOrderCommand(
            TenantId: Guid.Empty,
            MenuId: Guid.NewGuid(),
            FulfillmentMethod: "Pickup",
            CustomerName: "محمد حسینی",
            CustomerPhone: "09111111111",
            CustomerNote: null,
            DeliveryFee: 0m,
            TaxAmount: 0m,
            Items: new List<OrderItemInput>
            {
                new(Guid.NewGuid(), "نوشابه", 10000m, 1, null)
            });

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleViolationException>(() =>
            _handler.HandleAsync(command, CancellationToken.None));

        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockSmsSender.Verify(s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SmsSendContext>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_EmptyItems_ThrowsBusinessRuleViolation()
    {
        // Arrange
        var command = new PlaceOrderCommand(
            TenantId: Guid.NewGuid(),
            MenuId: Guid.NewGuid(),
            FulfillmentMethod: "DineIn",
            CustomerName: "فاطمه کریمی",
            CustomerPhone: "09222222222",
            CustomerNote: null,
            DeliveryFee: 0m,
            TaxAmount: 0m,
            Items: new List<OrderItemInput>());

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleViolationException>(() =>
            _handler.HandleAsync(command, CancellationToken.None));

        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockSmsSender.Verify(s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SmsSendContext>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_InvalidPhoneNumber_ThrowsBusinessRuleViolation()
    {
        // Arrange
        var command = new PlaceOrderCommand(
            TenantId: Guid.NewGuid(),
            MenuId: Guid.NewGuid(),
            FulfillmentMethod: "Pickup",
            CustomerName: "رضا محمدی",
            CustomerPhone: "invalid-phone",
            CustomerNote: null,
            DeliveryFee: 0m,
            TaxAmount: 0m,
            Items: new List<OrderItemInput>
            {
                new(Guid.NewGuid(), "ساندویچ", 50000m, 1, null)
            });

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleViolationException>(() =>
            _handler.HandleAsync(command, CancellationToken.None));

        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockSmsSender.Verify(s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SmsSendContext>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_MultipleItems_CreatesOrderWithAllItems()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var menuId = Guid.NewGuid();
        var orderNumber = "ORD-125";
        
        var command = new PlaceOrderCommand(
            TenantId: tenantId,
            MenuId: menuId,
            FulfillmentMethod: "Delivery",
            CustomerName: "حسن عباسی",
            CustomerPhone: "09333333333",
            CustomerNote: "سریع بیارید",
            DeliveryFee: 20000m,
            TaxAmount: 8000m,
            Items: new List<OrderItemInput>
            {
                new(Guid.NewGuid(), "پیتزا مخصوص", 200000m, 2, "اضافه پنیر"),
                new(Guid.NewGuid(), "نوشابه", 15000m, 3, null),
                new(Guid.NewGuid(), "سیب زمینی", 30000m, 1, "کاملا سرخ شده")
            });

        _mockNumberGenerator
            .Setup(g => g.GenerateAsync(It.IsAny<TenantId>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderNumber);

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockRealtimeNotifier
            .Setup(n => n.PublishOrderChangedAsync(It.IsAny<OrderRealtimeNotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockSmsSender
            .Setup(s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SmsSendContext>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _mockRepository.Verify(r => r.AddAsync(
            It.Is<Order>(o => o.Items.Count == 3),
            It.IsAny<CancellationToken>()), Times.Once);

        _mockSmsSender.Verify(s => s.SendAsync(
            "09333333333",
            It.Is<string>(msg => msg.Contains("503") && msg.Contains("تومان")), // Verify amount and currency
            It.IsAny<SmsSendContext>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
