using System.Collections.Generic;
using EazyMenu.Domain.Abstractions;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Domain.Events;

public sealed record MenuCategoriesReorderedDomainEvent(MenuId MenuId, IReadOnlyCollection<MenuCategoryId> OrderedCategoryIds) : DomainEventBase;
