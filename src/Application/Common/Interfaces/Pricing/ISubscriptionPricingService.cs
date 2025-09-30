using EazyMenu.Domain.Aggregates.Tenants;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Common.Interfaces.Pricing;

public interface ISubscriptionPricingService
{
    SubscriptionPriceQuote GetPriceQuote(SubscriptionPlan plan, bool useTrial, string? discountCode);
}

public sealed record SubscriptionPriceQuote(
    Money OriginalPrice,
    Money NetPrice,
    Percentage? DiscountPercentage,
    string? DiscountCode,
    bool IsTrial);
