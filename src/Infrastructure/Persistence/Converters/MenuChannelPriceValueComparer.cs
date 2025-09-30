using System;
using System.Collections.Generic;
using System.Linq;
using EazyMenu.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EazyMenu.Infrastructure.Persistence.Converters;

internal sealed class MenuChannelPriceValueComparer : ValueComparer<Dictionary<MenuChannel, Money>>
{
    public MenuChannelPriceValueComparer()
        : base(
            (left, right) => left!.Count == right!.Count && !left.Except(right).Any(),
            prices => prices!.Aggregate(0, (hash, pair) => HashCode.Combine(hash, pair.Key, pair.Value.Amount, pair.Value.Currency)),
            prices => prices!.ToDictionary(pair => pair.Key, pair => Money.From(pair.Value.Amount, pair.Value.Currency)))
    {
    }
}
