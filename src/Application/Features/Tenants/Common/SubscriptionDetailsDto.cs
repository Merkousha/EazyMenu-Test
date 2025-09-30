using System;
using EazyMenu.Domain.Aggregates.Tenants;

namespace EazyMenu.Application.Features.Tenants.Common;

public sealed record SubscriptionDetailsDto(
    Guid SubscriptionId,
    SubscriptionPlan Plan,
    SubscriptionStatus Status,
    decimal PriceAmount,
    string PriceCurrency,
    decimal NetPriceAmount,
    DateTime StartDateUtc,
    DateTime? EndDateUtc,
    bool IsTrial,
    decimal? DiscountPercentage,
    string? DiscountCode);
