using System;
using EazyMenu.Application.Common.Interfaces.Pricing;
using EazyMenu.Domain.Aggregates.Tenants;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Infrastructure.Pricing;

internal sealed class SubscriptionPricingService : ISubscriptionPricingService
{
    public SubscriptionPriceQuote GetPriceQuote(SubscriptionPlan plan, bool useTrial, string? discountCode)
    {
        var basePrice = ResolvePlanPrice(plan);

        if (useTrial)
        {
            return new SubscriptionPriceQuote(
                basePrice,
                Money.From(0, basePrice.Currency),
                null,
                null,
                true);
        }

        if (string.IsNullOrWhiteSpace(discountCode))
        {
            return new SubscriptionPriceQuote(basePrice, basePrice, null, null, false);
        }

        var trimmedCode = discountCode.Trim();
        var percentage = ResolveDiscountPercentage(trimmedCode);

        if (percentage is null)
        {
            return new SubscriptionPriceQuote(basePrice, basePrice, null, null, false);
        }

        var discountAmount = basePrice.Multiply(percentage.ToFraction());
        var netPrice = basePrice.Subtract(discountAmount);

        return new SubscriptionPriceQuote(basePrice, netPrice, percentage, trimmedCode, false);
    }

    private static Money ResolvePlanPrice(SubscriptionPlan plan)
    {
        return plan switch
        {
            SubscriptionPlan.Trial => Money.From(0),
            SubscriptionPlan.Starter => Money.From(990_000m),
            SubscriptionPlan.Pro => Money.From(1_990_000m),
            SubscriptionPlan.Enterprise => Money.From(4_990_000m),
            _ => Money.From(990_000m)
        };
    }

    private static Percentage? ResolveDiscountPercentage(string code)
    {
        return code.ToUpperInvariant() switch
        {
            "WELCOME10" => Percentage.From(10),
            "SPRING15" => Percentage.From(15),
            "SUMMER20" => Percentage.From(20),
            _ => null
        };
    }
}
