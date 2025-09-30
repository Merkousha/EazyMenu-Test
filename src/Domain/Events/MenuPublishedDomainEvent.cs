using EazyMenu.Domain.Abstractions;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Domain.Events;

public sealed record MenuPublishedDomainEvent(MenuId MenuId, TenantId TenantId, int Version) : DomainEventBase;
