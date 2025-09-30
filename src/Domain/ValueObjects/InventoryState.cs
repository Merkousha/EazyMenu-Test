using System;
using System.Collections.Generic;
using EazyMenu.Domain.Common;
using EazyMenu.Domain.Common.Guards;

namespace EazyMenu.Domain.ValueObjects;

public sealed class InventoryState : ValueObject
{
    private InventoryState(InventoryTrackingMode mode, int? quantity, int? threshold)
    {
        Mode = mode;
        Quantity = quantity;
        Threshold = threshold;
    }

    public InventoryTrackingMode Mode { get; }

    public int? Quantity { get; }

    public int? Threshold { get; }

    public bool IsAvailable => Mode == InventoryTrackingMode.Infinite || (Quantity ?? 0) > 0;

    public bool IsBelowThreshold => Mode == InventoryTrackingMode.Track && Threshold.HasValue && Quantity.HasValue && Quantity.Value <= Threshold.Value;

    public static InventoryState Infinite() => new(InventoryTrackingMode.Infinite, null, null);

    public static InventoryState Track(int quantity, int? threshold = null)
    {
        Guard.AgainstOutOfRange(quantity, 0, int.MaxValue, nameof(quantity));
        if (threshold.HasValue)
        {
            Guard.AgainstOutOfRange(threshold.Value, 0, quantity, nameof(threshold));
        }

        return new InventoryState(InventoryTrackingMode.Track, quantity, threshold ?? 0);
    }

    public InventoryState Increase(int amount)
    {
        Guard.AgainstOutOfRange(amount, 0, int.MaxValue, nameof(amount));

        if (Mode == InventoryTrackingMode.Infinite || amount == 0)
        {
            return this;
        }

        var newQuantity = checked(Quantity!.Value + amount);
        return new InventoryState(InventoryTrackingMode.Track, newQuantity, Threshold);
    }

    public InventoryState Decrease(int amount)
    {
        Guard.AgainstOutOfRange(amount, 0, int.MaxValue, nameof(amount));

        if (Mode == InventoryTrackingMode.Infinite || amount == 0)
        {
            return this;
        }

        var newQuantity = Quantity!.Value - amount;
        if (newQuantity < 0)
        {
            throw new InvalidOperationException("موجودی نمی‌تواند منفی شود.");
        }

        return new InventoryState(InventoryTrackingMode.Track, newQuantity, Threshold);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Mode;
        yield return Quantity;
        yield return Threshold;
    }
}

public enum InventoryTrackingMode : byte
{
    Infinite = 0,
    Track = 1
}
