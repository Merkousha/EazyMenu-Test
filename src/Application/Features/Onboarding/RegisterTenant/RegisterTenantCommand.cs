using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Domain.ValueObjects;
using EazyMenu.Application.Common.Interfaces.Provisioning;

namespace EazyMenu.Application.Features.Onboarding.RegisterTenant;

public sealed record RegisterTenantCommand(
    string RestaurantName,
    string ManagerEmail,
    string ManagerPhone,
    string PlanCode,
    string City,
    string Street,
    string PostalCode,
    bool UseTrial = false,
    string? DiscountCode = null) : ICommand<TenantProvisioningResult>;
