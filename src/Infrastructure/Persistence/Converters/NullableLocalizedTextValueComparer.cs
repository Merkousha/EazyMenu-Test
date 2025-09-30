using System;
using System.Collections.Generic;
using System.Linq;
using EazyMenu.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EazyMenu.Infrastructure.Persistence.Converters;

internal sealed class NullableLocalizedTextValueComparer : ValueComparer<LocalizedText?>
{
    public NullableLocalizedTextValueComparer()
        : base(
            (left, right) =>
                (left == null && right == null)
                || (left != null
                    && right != null
                    && left.Values.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
                        .SequenceEqual(right.Values.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))),
            text => text == null
                ? 0
                : text.Values.Aggregate(
                    0,
                    (hash, pair) => HashCode.Combine(
                        hash,
                        StringComparer.OrdinalIgnoreCase.GetHashCode(pair.Key),
                        StringComparer.Ordinal.GetHashCode(pair.Value))),
            text => text == null
                ? null
                : LocalizedText.FromDictionary(
                    text.Values.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase)))
    {
    }
}
