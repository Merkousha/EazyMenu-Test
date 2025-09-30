using System;
using EazyMenu.Domain.Aggregates.Payments;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Common.Interfaces.Provisioning;

public sealed record TenantProvisioningResult(
    TenantId TenantId,
    Guid SubscriptionId,
    ProvisionedPayment? Payment);

public sealed record ProvisionedPayment(
    PaymentId PaymentId,
    PaymentStatus Status,
    Uri? RedirectUri,
    string? GatewayAuthority);
