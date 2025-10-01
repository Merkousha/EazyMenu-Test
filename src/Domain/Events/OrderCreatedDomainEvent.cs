using System;
using EazyMenu.Domain.Abstractions;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Domain.Events;

public sealed record OrderCreatedDomainEvent(OrderId OrderId, TenantId TenantId, Guid MenuId, OrderStatus Status) : DomainEventBase;
