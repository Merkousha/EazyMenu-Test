using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EazyMenu.Domain.Abstractions;
using EazyMenu.Domain.Common.Exceptions;
using EazyMenu.Domain.Common.Guards;
using EazyMenu.Domain.Events;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Domain.Aggregates.Orders;

public sealed class Order : Entity<OrderId>, IAggregateRoot
{
    private readonly List<OrderItem> _items = new();

    private Order(
        OrderId id,
        TenantId tenantId,
        Guid menuId,
        string orderNumber,
        FulfillmentMethod fulfillmentMethod,
        string customerName,
        PhoneNumber customerPhone,
        string? customerNote,
        decimal deliveryFee,
        decimal taxAmount)
        : base(id)
    {
        Guard.AgainstDefault(tenantId, nameof(tenantId));
        Guard.AgainstDefault(menuId, nameof(menuId));
        Guard.AgainstNullOrWhiteSpace(orderNumber, nameof(orderNumber));
        Guard.AgainstNullOrWhiteSpace(customerName, nameof(customerName));
        Guard.AgainstNegative(deliveryFee, nameof(deliveryFee));
        Guard.AgainstNegative(taxAmount, nameof(taxAmount));

        TenantId = tenantId;
        MenuId = menuId;
        OrderNumber = orderNumber.Trim();
        FulfillmentMethod = fulfillmentMethod;
        CustomerName = customerName.Trim();
        CustomerPhone = customerPhone;
        CustomerNote = string.IsNullOrWhiteSpace(customerNote) ? null : customerNote.Trim();
        DeliveryFee = deliveryFee;
        TaxAmount = taxAmount;
        Status = OrderStatus.Pending;
        CreatedAtUtc = DateTime.UtcNow;
    }

    private Order()
    {
    }

    public TenantId TenantId { get; private set; }

    public Guid MenuId { get; private set; }

    public string OrderNumber { get; private set; } = null!;

    public OrderStatus Status { get; private set; }

    public FulfillmentMethod FulfillmentMethod { get; private set; }

    public string CustomerName { get; private set; } = null!;

    public PhoneNumber CustomerPhone { get; private set; } = null!;

    public string? CustomerNote { get; private set; }

    public decimal DeliveryFee { get; private set; }

    public decimal TaxAmount { get; private set; }

    public decimal SubtotalAmount { get; private set; }

    public decimal TotalAmount => SubtotalAmount + TaxAmount + DeliveryFee;

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? ConfirmedAtUtc { get; private set; }

    public DateTime? CompletedAtUtc { get; private set; }

    public DateTime? CancelledAtUtc { get; private set; }

    public string? CancellationReason { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => new ReadOnlyCollection<OrderItem>(_items.ToList());

    public static Order Create(
        TenantId tenantId,
        Guid menuId,
        string orderNumber,
        FulfillmentMethod fulfillmentMethod,
        string customerName,
        PhoneNumber customerPhone,
        string? customerNote,
        decimal deliveryFee,
        decimal taxAmount,
        IEnumerable<OrderItemDraft> items)
    {
        Guard.AgainstNull(items, nameof(items));
        var drafts = items.ToList();
        if (drafts.Count == 0)
        {
            throw new DomainException("ثبت سفارش بدون آیتم مجاز نیست.");
        }

        var order = new Order(OrderId.New(), tenantId, menuId, orderNumber, fulfillmentMethod, customerName, customerPhone, customerNote, deliveryFee, taxAmount);

        foreach (var draft in drafts)
        {
            order.AddItemInternal(draft);
        }

        order.RecalculateTotals();
        order.RaiseDomainEvent(new OrderCreatedDomainEvent(order.Id, tenantId, menuId, order.Status));
        return order;
    }

    public void Confirm()
    {
        EnsureNotCancelled();

        if (Status != OrderStatus.Pending)
        {
            throw new DomainException("تنها سفارش‌های در انتظار قابل تأیید هستند.");
        }

        Status = OrderStatus.Confirmed;
        ConfirmedAtUtc = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (Status != OrderStatus.Confirmed)
        {
            throw new DomainException("تنها سفارش‌های تأیید شده قابل تکمیل هستند.");
        }

        Status = OrderStatus.Completed;
        CompletedAtUtc = DateTime.UtcNow;
    }

    public void Cancel(string? reason)
    {
        if (Status == OrderStatus.Completed)
        {
            throw new DomainException("سفارش تکمیل‌شده قابل لغو نیست.");
        }

        if (Status == OrderStatus.Cancelled)
        {
            return;
        }

        Status = OrderStatus.Cancelled;
        CancelledAtUtc = DateTime.UtcNow;
        CancellationReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
    }

    public void UpdateCustomerNote(string? note)
    {
        CustomerNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
    }

    internal void AddOrUpdateItem(Guid menuItemId, string displayName, decimal unitPrice, int quantity, string? note)
    {
        EnsureMutable();

        var existing = _items.SingleOrDefault(item => item.MenuItemId == menuItemId);
        if (existing is null)
        {
            AddItemInternal(new OrderItemDraft(menuItemId, displayName, unitPrice, quantity, note));
        }
        else
        {
            existing.Update(displayName, unitPrice, quantity, note);
        }

        RecalculateTotals();
    }

    internal void RemoveItem(Guid menuItemId)
    {
        EnsureMutable();

        var item = _items.SingleOrDefault(i => i.MenuItemId == menuItemId);
        if (item is null)
        {
            throw new DomainException("آیتم مورد نظر در سفارش یافت نشد.");
        }

        _items.Remove(item);
        if (_items.Count == 0)
        {
            throw new DomainException("سفارش باید حداقل یک آیتم داشته باشد.");
        }

        RecalculateTotals();
    }

    private void AddItemInternal(OrderItemDraft draft)
    {
        Guard.AgainstDefault(draft.MenuItemId, nameof(draft.MenuItemId));
        Guard.AgainstNullOrWhiteSpace(draft.DisplayName, nameof(draft.DisplayName));
        Guard.AgainstNegative(draft.UnitPrice, nameof(draft.UnitPrice));
        Guard.AgainstOutOfRange(draft.Quantity, 1, 1000, nameof(draft.Quantity));

        var item = OrderItem.Create(
            draft.MenuItemId,
            draft.DisplayName,
            draft.UnitPrice,
            draft.Quantity,
            draft.Note);

        _items.Add(item);
    }

    private void RecalculateTotals()
    {
        SubtotalAmount = _items.Sum(item => item.TotalAmount);
    }

    private void EnsureMutable()
    {
        EnsureNotCancelled();
        if (Status == OrderStatus.Completed)
        {
            throw new DomainException("سفارش تکمیل‌شده قابل ویرایش نیست.");
        }
    }

    private void EnsureNotCancelled()
    {
        if (Status == OrderStatus.Cancelled)
        {
            throw new DomainException("سفارش لغوشده قابل ویرایش نیست.");
        }
    }

    public readonly record struct OrderItemDraft(
        Guid MenuItemId,
        string DisplayName,
        decimal UnitPrice,
        int Quantity,
        string? Note);
}
