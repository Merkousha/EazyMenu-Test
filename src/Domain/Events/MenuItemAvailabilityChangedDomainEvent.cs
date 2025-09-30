using EazyMenu.Domain.Abstractions;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Domain.Events;

public sealed record MenuItemAvailabilityChangedDomainEvent(MenuId MenuId, MenuItemId MenuItemId, bool IsAvailable) : DomainEventBase;
