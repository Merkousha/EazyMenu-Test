using System;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Features.Tenants.Subscriptions.GetActiveSubscription;
using EazyMenu.Domain.Aggregates.Tenants;
using EazyMenu.Domain.ValueObjects;
using Xunit;

namespace EazyMenu.UnitTests.Application.Features.Tenants.Subscriptions;

public sealed class GetActiveSubscriptionQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldReturnActiveSubscriptionDetails()
    {
        var tenant = CreateTenantWithDiscountedSubscription();
        var repository = new InMemoryTenantRepository(tenant);
        var handler = new GetActiveSubscriptionQueryHandler(repository);
        var command = new GetActiveSubscriptionQuery(tenant.Id.Value);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(tenant.ActiveSubscription!.Id, result!.SubscriptionId);
        Assert.Equal(tenant.ActiveSubscription.Plan, result.Plan);
        Assert.Equal(tenant.ActiveSubscription.Status, result.Status);
        Assert.Equal(tenant.ActiveSubscription.Price.Amount, result.PriceAmount);
        Assert.Equal(tenant.ActiveSubscription.Price.Currency, result.PriceCurrency);
        Assert.Equal(tenant.ActiveSubscription.NetPrice.Amount, result.NetPriceAmount);
        Assert.Equal(tenant.ActiveSubscription.StartDateUtc, result.StartDateUtc);
        Assert.Equal(tenant.ActiveSubscription.EndDateUtc, result.EndDateUtc);
        Assert.Equal(tenant.ActiveSubscription.IsTrial, result.IsTrial);
        Assert.Equal(tenant.ActiveSubscription.DiscountPercentage!.Value, result.DiscountPercentage);
        Assert.Equal(tenant.ActiveSubscription.DiscountCode, result.DiscountCode);
    }

    [Fact]
    public async Task HandleAsync_WhenNoActiveSubscription_ShouldReturnNull()
    {
        var tenant = CreateTenant();
        var repository = new InMemoryTenantRepository(tenant);
        var handler = new GetActiveSubscriptionQueryHandler(repository);
        var command = new GetActiveSubscriptionQuery(tenant.Id.Value);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task HandleAsync_WhenTenantNotFound_ShouldThrow()
    {
        var repository = new InMemoryTenantRepository(null);
        var handler = new GetActiveSubscriptionQueryHandler(repository);
        var command = new GetActiveSubscriptionQuery(Guid.NewGuid());

        await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenTenantIdInvalid_ShouldThrow()
    {
        var repository = new InMemoryTenantRepository(CreateTenant());
        var handler = new GetActiveSubscriptionQueryHandler(repository);
        var command = new GetActiveSubscriptionQuery(Guid.Empty);

        await Assert.ThrowsAsync<BusinessRuleViolationException>(() => handler.HandleAsync(command, CancellationToken.None));
    }

    private static Tenant CreateTenant()
    {
        var brand = BrandProfile.Create("Cafe");
        var email = Email.Create("owner@example.com");
        var phone = PhoneNumber.Create("+989121234567");
        return Tenant.Register("کافه نمونه", brand, email, phone, Address.Create("تهران", "خیابان انقلاب", "00000"));
    }

    private static Tenant CreateTenantWithDiscountedSubscription()
    {
        var tenant = CreateTenant();
        var start = DateTime.UtcNow.AddDays(-5);
        var end = DateTime.UtcNow.AddDays(25);
        var subscription = Subscription.Create(SubscriptionPlan.Enterprise, Money.From(1200000m), start, end);
        subscription.ApplyDiscount(Percentage.From(10m), "AUTUMN10");
        tenant.ActivateSubscription(subscription);
        return tenant;
    }

    private sealed class InMemoryTenantRepository : ITenantRepository
    {
        private readonly Tenant? _tenant;

        public InMemoryTenantRepository(Tenant? tenant)
        {
            _tenant = tenant;
        }

        public Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Tenant?> GetByIdAsync(TenantId tenantId, CancellationToken cancellationToken = default)
        {
            if (_tenant is null)
            {
                return Task.FromResult<Tenant?>(null);
            }

            return Task.FromResult<Tenant?>(_tenant.Id == tenantId ? _tenant : null);
        }

        public Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
