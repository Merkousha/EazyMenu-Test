using System;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Domain.Aggregates.Tenants;

namespace EazyMenu.Application.Features.Tenants.Subscriptions.RenewSubscription;

public sealed record RenewSubscriptionCommand(
    Guid TenantId,
    SubscriptionPlan Plan,
    decimal PriceAmount,
    string? PriceCurrency,
    DateTime NewStartDateUtc,
    DateTime? NewEndDateUtc,
    bool AsTrial,
    decimal? DiscountPercentage,
    string? DiscountCode) : ICommand<Guid>;
