using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Common.Interfaces.Provisioning;

public sealed record TenantProvisioningRequest(
    string RestaurantName,
    string ManagerEmail,
    string ManagerPhone,
    string PlanCode,
    Address HeadquartersAddress,
    bool UseTrial,
    string? DiscountCode);
