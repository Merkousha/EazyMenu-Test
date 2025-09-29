using EazyMenu.Domain.Abstractions;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Domain.Events;

public sealed record TenantRegisteredDomainEvent(TenantId TenantId, string BusinessName) : DomainEventBase;
