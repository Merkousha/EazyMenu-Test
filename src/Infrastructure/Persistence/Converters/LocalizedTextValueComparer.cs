using System;
using System.Collections.Generic;
using System.Linq;
using EazyMenu.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EazyMenu.Infrastructure.Persistence.Converters;

internal sealed class LocalizedTextValueComparer : ValueComparer<LocalizedText>
{
    public LocalizedTextValueComparer()
        : base(
                (left, right) => left!.Values.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
                    .SequenceEqual(right!.Values.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)),
                text => text!.Values.Aggregate(0, (hash, pair) => HashCode.Combine(hash, StringComparer.OrdinalIgnoreCase.GetHashCode(pair.Key), StringComparer.Ordinal.GetHashCode(pair.Value))),
                text => LocalizedText.FromDictionary(text!.Values.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase)))
    {
    }
}