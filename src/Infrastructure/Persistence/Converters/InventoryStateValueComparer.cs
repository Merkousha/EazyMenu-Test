using System;
using EazyMenu.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EazyMenu.Infrastructure.Persistence.Converters;

internal sealed class InventoryStateValueComparer : ValueComparer<InventoryState>
{
    public InventoryStateValueComparer()
        : base(
            (left, right) => left!.Mode == right!.Mode && left.Quantity == right.Quantity && left.Threshold == right.Threshold,
            state => HashCode.Combine(state!.Mode, state.Quantity, state.Threshold),
            state => state!.Mode == InventoryTrackingMode.Infinite
                ? InventoryState.Infinite()
                : InventoryState.Track(state.Quantity ?? 0, state.Threshold))
    {
    }
}
