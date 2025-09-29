using System;

namespace EazyMenu.Domain.Abstractions;

public abstract record DomainEventBase : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();

    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
