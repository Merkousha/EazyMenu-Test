using System;
using EazyMenu.Domain.Abstractions;
using EazyMenu.Domain.Common.Exceptions;
using EazyMenu.Domain.Common.Guards;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Domain.Aggregates.Orders;

public sealed class OrderItem : Entity<OrderItemId>
{
    private OrderItem(OrderItemId id, Guid menuItemId, string displayName, decimal unitPrice, int quantity, string? note)
        : base(id)
    {
        Guard.AgainstDefault(menuItemId, nameof(menuItemId));
        Guard.AgainstNullOrWhiteSpace(displayName, nameof(displayName));
        Guard.AgainstNegative(unitPrice, nameof(unitPrice));
        Guard.AgainstOutOfRange(quantity, 1, 1000, nameof(quantity));

        MenuItemId = menuItemId;
        DisplayName = displayName.Trim();
        UnitPrice = unitPrice;
        Quantity = quantity;
        Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
    }

    private OrderItem()
    {
    }

    public Guid MenuItemId { get; private set; }

    public string DisplayName { get; private set; } = null!;

    public decimal UnitPrice { get; private set; }

    public int Quantity { get; private set; }

    public string? Note { get; private set; }

    public decimal TotalAmount => UnitPrice * Quantity;

    internal static OrderItem Create(Guid menuItemId, string displayName, decimal unitPrice, int quantity, string? note)
    {
        return new OrderItem(OrderItemId.New(), menuItemId, displayName, unitPrice, quantity, note);
    }

    internal void Update(string displayName, decimal unitPrice, int quantity, string? note)
    {
        Guard.AgainstNullOrWhiteSpace(displayName, nameof(displayName));
        Guard.AgainstNegative(unitPrice, nameof(unitPrice));
        Guard.AgainstOutOfRange(quantity, 1, 1000, nameof(quantity));

        DisplayName = displayName.Trim();
        UnitPrice = unitPrice;
        Quantity = quantity;
        Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
    }

    internal void IncreaseQuantity(int amount)
    {
        Guard.AgainstOutOfRange(amount, 1, 1000, nameof(amount));
        var newQuantity = Quantity + amount;
        Guard.AgainstOutOfRange(newQuantity, 1, 5000, nameof(newQuantity));
        Quantity = newQuantity;
    }

    internal void DecreaseQuantity(int amount)
    {
        Guard.AgainstOutOfRange(amount, 1, 1000, nameof(amount));
        var newQuantity = Quantity - amount;
        if (newQuantity <= 0)
        {
            throw new DomainException("مقدار آیتم نمی‌تواند صفر یا منفی باشد.");
        }

        Quantity = newQuantity;
    }
}
