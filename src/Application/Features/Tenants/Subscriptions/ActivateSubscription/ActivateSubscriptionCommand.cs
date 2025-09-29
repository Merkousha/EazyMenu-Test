using System;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Domain.Aggregates.Tenants;

namespace EazyMenu.Application.Features.Tenants.Subscriptions.ActivateSubscription;

public sealed record ActivateSubscriptionCommand(
    Guid TenantId,
    SubscriptionPlan Plan,
    decimal PriceAmount,
    string? PriceCurrency,
    DateTime StartDateUtc,
    DateTime? EndDateUtc,
    bool IsTrial,
    decimal? DiscountPercentage,
    string? DiscountCode) : ICommand<Guid>;
