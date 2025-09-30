using EazyMenu.Domain.Abstractions;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Domain.Events;

public sealed record MenuItemPriceChangedDomainEvent(MenuId MenuId, MenuItemId MenuItemId, Money NewPrice) : DomainEventBase;
