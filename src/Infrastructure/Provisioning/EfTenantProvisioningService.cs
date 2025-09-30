using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Common.Interfaces;
using EazyMenu.Application.Common.Interfaces.Payments;
using EazyMenu.Application.Common.Interfaces.Pricing;
using EazyMenu.Application.Common.Interfaces.Provisioning;
using EazyMenu.Application.Common.Time;
using EazyMenu.Domain.Aggregates.Payments;
using EazyMenu.Domain.Aggregates.Tenants;
using EazyMenu.Domain.ValueObjects;
using EazyMenu.Infrastructure.Payments;
using EazyMenu.Infrastructure.Persistence;
using EazyMenu.Infrastructure.Persistence.Models;

namespace EazyMenu.Infrastructure.Provisioning;

internal sealed class EfTenantProvisioningService : ITenantProvisioningService
{
    private readonly EazyMenuDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ISubscriptionPricingService _pricingService;
    private readonly IPaymentGatewayClient _paymentGatewayClient;
    private readonly Uri _callbackUri;

    public EfTenantProvisioningService(
        EazyMenuDbContext dbContext,
        IDateTimeProvider dateTimeProvider,
        ISubscriptionPricingService pricingService,
        IPaymentGatewayClient paymentGatewayClient,
        PaymentGatewayOptions paymentGatewayOptions)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
        _pricingService = pricingService;
        _paymentGatewayClient = paymentGatewayClient;
        _callbackUri = paymentGatewayOptions.GetCallbackUri();
    }

    public async Task<TenantProvisioningResult> ProvisionAsync(TenantProvisioningRequest request, CancellationToken cancellationToken = default)
    {
        var plan = ResolvePlan(request.PlanCode);
        var priceQuote = _pricingService.GetPriceQuote(plan, request.UseTrial, request.DiscountCode);

        var startUtc = _dateTimeProvider.UtcNow;
    DateTime? endUtc = priceQuote.IsTrial ? startUtc.AddDays(14) : null;

        var tenant = Tenant.Register(
            request.RestaurantName,
            BrandProfile.Create(request.RestaurantName),
            Email.Create(request.ManagerEmail),
            PhoneNumber.Create(request.ManagerPhone),
            request.HeadquartersAddress);

        var subscription = Subscription.Create(plan, priceQuote.NetPrice, startUtc, endUtc, priceQuote.IsTrial);

        if (priceQuote.DiscountPercentage is not null)
        {
            subscription.ApplyDiscount(priceQuote.DiscountPercentage, priceQuote.DiscountCode);
        }

        PaymentTransaction? paymentTransaction = null;
        ProvisionedPayment? paymentResult = null;

        if (!priceQuote.IsTrial && !priceQuote.NetPrice.IsZero())
        {
            tenant.RegisterPendingSubscription(subscription);

            paymentTransaction = PaymentTransaction.Issue(
                tenant.Id,
                subscription.Id,
                priceQuote.NetPrice,
                PaymentMethod.Zarinpal,
                $"حق اشتراک پلن {plan}",
                startUtc,
                priceQuote.OriginalPrice,
                priceQuote.DiscountPercentage,
                priceQuote.DiscountCode,
                null);

            var metadata = new Dictionary<string, string>
            {
                ["tenantId"] = tenant.Id.Value.ToString(),
                ["subscriptionId"] = subscription.Id.ToString(),
                ["plan"] = plan.ToString()
            };

            var paymentRequest = new PaymentGatewayRequest(
                tenant.Id,
                subscription.Id,
                priceQuote.NetPrice,
                $"حق اشتراک پلن {plan}",
                _callbackUri,
                request.ManagerEmail,
                request.ManagerPhone,
                priceQuote.DiscountCode,
                metadata);

            var gatewayResponse = await _paymentGatewayClient.CreatePaymentAsync(paymentRequest, cancellationToken);
            paymentTransaction.AttachGatewayAuthority(gatewayResponse.Authority);

            paymentResult = new ProvisionedPayment(
                paymentTransaction.Id,
                paymentTransaction.Status,
                gatewayResponse.RedirectUri,
                paymentTransaction.GatewayAuthority);

            _dbContext.PaymentTransactions.Add(paymentTransaction);
        }
        else
        {
            tenant.ActivateSubscription(subscription);
        }

        var record = TenantProvisioningRecord.Create(
            tenant.Id.Value,
            request.RestaurantName,
            Slugify(request.RestaurantName),
            request.ManagerEmail,
            request.ManagerPhone,
            request.PlanCode,
            request.HeadquartersAddress.City,
            request.HeadquartersAddress.Street,
            request.HeadquartersAddress.PostalCode,
            priceQuote.IsTrial,
            priceQuote.DiscountCode,
            _dateTimeProvider.UtcNow,
            subscription.Id,
            paymentTransaction?.Id.Value);

        _dbContext.Tenants.Add(tenant);
        await _dbContext.TenantProvisionings.AddAsync(record, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new TenantProvisioningResult(tenant.Id, subscription.Id, paymentResult);
    }

    private static SubscriptionPlan ResolvePlan(string planCode)
    {
        if (Enum.TryParse<SubscriptionPlan>(planCode, true, out var parsed))
        {
            return parsed;
        }

        return SubscriptionPlan.Starter;
    }

    private static string Slugify(string value)
    {
        return value
            .Trim()
            .ToLowerInvariant()
            .Replace(' ', '-');
    }
}
