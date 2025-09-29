using System;

namespace EazyMenu.Domain.Abstractions;

/// <summary>
/// Represents a domain event raised by an aggregate.
/// </summary>
public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccurredOnUtc { get; }
}
