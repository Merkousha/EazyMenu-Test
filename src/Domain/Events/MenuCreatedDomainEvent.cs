using EazyMenu.Domain.Abstractions;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Domain.Events;

public sealed record MenuCreatedDomainEvent(MenuId MenuId, TenantId TenantId) : DomainEventBase;
