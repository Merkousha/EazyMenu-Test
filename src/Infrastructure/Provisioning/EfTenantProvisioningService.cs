using System;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Common.Interfaces;
using EazyMenu.Application.Common.Interfaces.Provisioning;
using EazyMenu.Application.Common.Time;
using EazyMenu.Domain.Aggregates.Tenants;
using EazyMenu.Domain.ValueObjects;
using EazyMenu.Infrastructure.Persistence;
using EazyMenu.Infrastructure.Persistence.Models;

namespace EazyMenu.Infrastructure.Provisioning;

internal sealed class EfTenantProvisioningService : ITenantProvisioningService
{
    private readonly EazyMenuDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public EfTenantProvisioningService(EazyMenuDbContext dbContext, IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<TenantId> ProvisionAsync(TenantProvisioningRequest request, CancellationToken cancellationToken = default)
    {
    var plan = ResolvePlan(request.PlanCode);
    var startUtc = _dateTimeProvider.UtcNow;
    DateTime? endUtc = request.UseTrial ? startUtc.AddDays(14) : null;
        var price = request.UseTrial ? Money.From(0) : ResolvePlanPrice(plan);

        var tenant = Tenant.Register(
            request.RestaurantName,
            BrandProfile.Create(request.RestaurantName),
            Email.Create(request.ManagerEmail),
            PhoneNumber.Create(request.ManagerPhone),
            request.HeadquartersAddress);

        var subscription = Subscription.Create(plan, price, startUtc, endUtc, request.UseTrial);
        tenant.ActivateSubscription(subscription);

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
            request.UseTrial,
            request.DiscountCode,
            _dateTimeProvider.UtcNow);

        _dbContext.Tenants.Add(tenant);
        await _dbContext.TenantProvisionings.AddAsync(record, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return tenant.Id;
    }

    private static SubscriptionPlan ResolvePlan(string planCode)
    {
        if (Enum.TryParse<SubscriptionPlan>(planCode, true, out var parsed))
        {
            return parsed;
        }

        return SubscriptionPlan.Starter;
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

    private static string Slugify(string value)
    {
        return value
            .Trim()
            .ToLowerInvariant()
            .Replace(' ', '-');
    }
}
