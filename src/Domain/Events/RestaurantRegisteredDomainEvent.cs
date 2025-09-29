using System;
using EazyMenu.Domain.Abstractions;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Domain.Events;

public sealed class RestaurantRegisteredDomainEvent : IDomainEvent
{
    public RestaurantRegisteredDomainEvent(TenantId tenantId, string restaurantName)
    {
        TenantId = tenantId;
        RestaurantName = restaurantName;
        OccurredOnUtc = DateTime.UtcNow;
        Id = Guid.NewGuid();
    }

    public Guid Id { get; }
    public DateTime OccurredOnUtc { get; }
    public TenantId TenantId { get; }
    public string RestaurantName { get; }
}
