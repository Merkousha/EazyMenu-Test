using EazyMenu.Domain.Abstractions;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Domain.Events;

public sealed record BranchCreatedDomainEvent(TenantId TenantId, BranchId BranchId, string BranchName) : DomainEventBase;
