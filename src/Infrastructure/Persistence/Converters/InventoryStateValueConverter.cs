using System;
using EazyMenu.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EazyMenu.Infrastructure.Persistence.Converters;

internal sealed class InventoryStateValueConverter : ValueConverter<InventoryState, string>
{
    public InventoryStateValueConverter()
        : base(state => Serialize(state), value => Deserialize(value))
    {
    }

    private static string Serialize(InventoryState state)
    {
        var mode = ((byte)state.Mode).ToString();
        var quantity = state.Quantity?.ToString() ?? string.Empty;
        var threshold = state.Threshold?.ToString() ?? string.Empty;
        return string.Join('|', mode, quantity, threshold);
    }

    private static InventoryState Deserialize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return InventoryState.Infinite();
        }

        var parts = value.Split('|');
        if (parts.Length == 0)
        {
            return InventoryState.Infinite();
        }

        var mode = (InventoryTrackingMode)Convert.ToByte(parts[0]);
        if (mode == InventoryTrackingMode.Infinite)
        {
            return InventoryState.Infinite();
        }

        var quantity = parts.Length > 1 && int.TryParse(parts[1], out var qty) ? qty : 0;
        var threshold = parts.Length > 2 && int.TryParse(parts[2], out var th) ? th : (int?)null;
        return InventoryState.Track(quantity, threshold);
    }
}
