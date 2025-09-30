using System;

namespace EazyMenu.Infrastructure.Persistence.Models;

public sealed class TenantProvisioningRecord
{
    public Guid TenantId { get; private set; }

    public Guid SubscriptionId { get; private set; }

    public Guid? PaymentId { get; private set; }

    public string RestaurantName { get; private set; } = string.Empty;

    public string RestaurantSlug { get; private set; } = string.Empty;

    public string ManagerEmail { get; private set; } = string.Empty;

    public string ManagerPhone { get; private set; } = string.Empty;

    public string PlanCode { get; private set; } = string.Empty;

    public string City { get; private set; } = string.Empty;

    public string Street { get; private set; } = string.Empty;

    public string PostalCode { get; private set; } = string.Empty;

    public bool UseTrial { get; private set; }

    public string? DiscountCode { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private TenantProvisioningRecord()
    {
    }

    private TenantProvisioningRecord(
        Guid tenantId,
        string restaurantName,
        string restaurantSlug,
        string managerEmail,
        string managerPhone,
        string planCode,
        string city,
        string street,
        string postalCode,
        bool useTrial,
        string? discountCode,
        DateTime createdAtUtc,
        Guid subscriptionId,
        Guid? paymentId)
    {
        TenantId = tenantId;
        SubscriptionId = subscriptionId;
        PaymentId = paymentId;
        RestaurantName = restaurantName;
        RestaurantSlug = restaurantSlug;
        ManagerEmail = managerEmail;
        ManagerPhone = managerPhone;
        PlanCode = planCode;
        City = city;
        Street = street;
        PostalCode = postalCode;
        UseTrial = useTrial;
        DiscountCode = string.IsNullOrWhiteSpace(discountCode) ? null : discountCode.Trim();
        CreatedAtUtc = createdAtUtc;
    }

    public static TenantProvisioningRecord Create(
        Guid tenantId,
        string restaurantName,
        string restaurantSlug,
        string managerEmail,
        string managerPhone,
        string planCode,
        string city,
        string street,
        string postalCode,
        bool useTrial,
        string? discountCode,
        DateTime createdAtUtc,
        Guid subscriptionId,
        Guid? paymentId)
    {
        return new TenantProvisioningRecord(
            tenantId,
            restaurantName.Trim(),
            restaurantSlug.Trim(),
            managerEmail.Trim(),
            managerPhone.Trim(),
            planCode.Trim(),
            city.Trim(),
            street.Trim(),
            postalCode.Trim(),
            useTrial,
            discountCode,
            createdAtUtc,
            subscriptionId,
            paymentId);
    }
}
