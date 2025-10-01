using System;
using EazyMenu.Domain.Abstractions;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Domain.Aggregates.Users.Events;

/// <summary>
/// رویداد ایجاد کاربر جدید.
/// </summary>
public sealed record UserCreatedDomainEvent(
    UserId UserId,
    TenantId TenantId,
    string Email,
    UserRole Role,
    DateTime OccurredOnUtc) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
}
