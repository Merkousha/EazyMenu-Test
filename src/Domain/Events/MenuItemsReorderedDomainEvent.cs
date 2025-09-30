using System.Collections.Generic;
using EazyMenu.Domain.Abstractions;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Domain.Events;

public sealed record MenuItemsReorderedDomainEvent(MenuId MenuId, MenuCategoryId CategoryId, IReadOnlyCollection<MenuItemId> OrderedItemIds) : DomainEventBase;
