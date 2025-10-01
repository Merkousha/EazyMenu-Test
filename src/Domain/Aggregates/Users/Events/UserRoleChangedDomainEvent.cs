using System;
using EazyMenu.Domain.Abstractions;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Domain.Aggregates.Users.Events;

/// <summary>
/// رویداد تغییر نقش کاربر.
/// </summary>
public sealed record UserRoleChangedDomainEvent(
    UserId UserId,
    UserRole OldRole,
    UserRole NewRole,
    DateTime OccurredOnUtc) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
}
