using System;
using EazyMenu.Domain.Abstractions;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Domain.Aggregates.Users.Events;

/// <summary>
/// رویداد تغییر رمز عبور کاربر.
/// </summary>
public sealed record UserPasswordChangedDomainEvent(
    UserId UserId,
    DateTime OccurredOnUtc) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
}
