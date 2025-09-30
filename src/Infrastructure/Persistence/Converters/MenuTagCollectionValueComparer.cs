using System;
using System.Collections.Generic;
using System.Linq;
using EazyMenu.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EazyMenu.Infrastructure.Persistence.Converters;

internal sealed class MenuTagCollectionValueComparer : ValueComparer<HashSet<MenuTag>>
{
    public MenuTagCollectionValueComparer()
        : base(
            (left, right) => left!.SetEquals(right!),
            tags => tags!.Aggregate(0, (hash, tag) => HashCode.Combine(hash, tag)),
            tags => new HashSet<MenuTag>(tags!))
    {
    }
}
