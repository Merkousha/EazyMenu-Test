using System;
using EazyMenu.Domain.Abstractions;
using EazyMenu.Domain.Aggregates.Tenants;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Domain.Events;

public sealed record SubscriptionActivatedDomainEvent(
    TenantId TenantId,
    Guid SubscriptionId,
    SubscriptionPlan Plan,
    DateTime ActivatedOnUtc) : DomainEventBase;
