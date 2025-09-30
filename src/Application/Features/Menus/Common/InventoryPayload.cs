using System;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Menus.Common;

public sealed record InventoryPayload(string Mode, int? Quantity, int? Threshold)
{
    public InventoryState ToInventoryState()
    {
        if (string.IsNullOrWhiteSpace(Mode))
        {
            throw new BusinessRuleViolationException("حالت موجودی مشخص نشده است.");
        }

        return Mode.Trim().Equals("Track", StringComparison.OrdinalIgnoreCase)
            ? InventoryState.Track(Quantity ?? 0, Threshold)
            : InventoryState.Infinite();
    }
}
