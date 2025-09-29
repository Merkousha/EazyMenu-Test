using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Onboarding.RegisterTenant;

public sealed record RegisterTenantCommand(
    string RestaurantName,
    string ManagerEmail,
    string PlanCode) : ICommand<TenantId>;
