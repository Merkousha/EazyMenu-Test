using System;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Features.Tenants.Subscriptions.RenewSubscription;
using EazyMenu.Domain.Aggregates.Tenants;
using EazyMenu.Domain.ValueObjects;
using Xunit;

namespace EazyMenu.UnitTests.Application.Features.Tenants.Subscriptions;

public sealed class RenewSubscriptionCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldRenewSubscriptionAndApplyDiscount()
    {
        var tenant = CreateTenantWithActiveSubscription();
        var repository = new InMemoryTenantRepository(tenant);
        var handler = new RenewSubscriptionCommandHandler(repository, new FakeUnitOfWork());
        var activeSubscription = tenant.ActiveSubscription!;
        var newStart = activeSubscription.EndDateUtc!.Value;
        var newEnd = newStart.AddMonths(1);

        var command = new RenewSubscriptionCommand(
            tenant.Id.Value,
            SubscriptionPlan.Enterprise,
            800000m,
            Money.DefaultCurrency,
            newStart,
            newEnd,
            AsTrial: false,
            DiscountPercentage: 15m,
            DiscountCode: "FALL15");

        var renewedId = await handler.HandleAsync(command, CancellationToken.None);

        Assert.True(repository.WasUpdated);
        Assert.Equal(renewedId, tenant.ActiveSubscription!.Id);
    Assert.Equal(SubscriptionPlan.Enterprise, tenant.ActiveSubscription.Plan);
        Assert.Equal(800000m, tenant.ActiveSubscription.Price.Amount);
        Assert.Equal(Money.DefaultCurrency, tenant.ActiveSubscription.Price.Currency);
        Assert.Equal(newStart, tenant.ActiveSubscription.StartDateUtc);
        Assert.Equal(newEnd, tenant.ActiveSubscription.EndDateUtc);
        Assert.Equal(15m, tenant.ActiveSubscription.DiscountPercentage!.Value);
        Assert.Equal("FALL15", tenant.ActiveSubscription.DiscountCode);

        var expectedNetAmount = 800000m - (800000m * 0.15m);
        Assert.Equal(expectedNetAmount, tenant.ActiveSubscription.NetPrice.Amount);
    }

    [Fact]
    public async Task HandleAsync_WhenTenantNotFound_ShouldThrow()
    {
        var repository = new InMemoryTenantRepository(null);
        var handler = new RenewSubscriptionCommandHandler(repository, new FakeUnitOfWork());
        var command = new RenewSubscriptionCommand(
            Guid.NewGuid(),
            SubscriptionPlan.Starter,
            500000m,
            Money.DefaultCurrency,
            DateTime.UtcNow.AddMonths(1),
            DateTime.UtcNow.AddMonths(2),
            AsTrial: false,
            DiscountPercentage: null,
            DiscountCode: null);

        await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenNoActiveSubscription_ShouldThrow()
    {
        var tenant = CreateTenant();
        var repository = new InMemoryTenantRepository(tenant);
        var handler = new RenewSubscriptionCommandHandler(repository, new FakeUnitOfWork());
        var command = new RenewSubscriptionCommand(
            tenant.Id.Value,
            SubscriptionPlan.Starter,
            500000m,
            Money.DefaultCurrency,
            DateTime.UtcNow.AddMonths(1),
            DateTime.UtcNow.AddMonths(2),
            AsTrial: false,
            DiscountPercentage: null,
            DiscountCode: null);

        await Assert.ThrowsAsync<BusinessRuleViolationException>(() => handler.HandleAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenRenewalStartsBeforeActiveEnd_ShouldThrow()
    {
        var tenant = CreateTenantWithActiveSubscription();
        var repository = new InMemoryTenantRepository(tenant);
        var handler = new RenewSubscriptionCommandHandler(repository, new FakeUnitOfWork());
        var activeSubscription = tenant.ActiveSubscription!;
        var command = new RenewSubscriptionCommand(
            tenant.Id.Value,
            SubscriptionPlan.Pro,
            800000m,
            Money.DefaultCurrency,
            activeSubscription.StartDateUtc.AddDays(1),
            activeSubscription.EndDateUtc?.AddMonths(1),
            AsTrial: false,
            DiscountPercentage: null,
            DiscountCode: null);

        await Assert.ThrowsAsync<BusinessRuleViolationException>(() => handler.HandleAsync(command, CancellationToken.None));
    }

    private static Tenant CreateTenant()
    {
        var brand = BrandProfile.Create("Cafe");
        var email = Email.Create("owner@example.com");
        var phone = PhoneNumber.Create("+989121234567");
        return Tenant.Register("کافه نمونه", brand, email, phone, Address.Create("تهران", "خیابان انقلاب", "00000"));
    }

    private static Tenant CreateTenantWithActiveSubscription()
    {
        var tenant = CreateTenant();
        var start = DateTime.UtcNow.AddMonths(-1);
        var end = DateTime.UtcNow.AddMonths(1);
    var subscription = Subscription.Create(SubscriptionPlan.Starter, Money.From(600000m), start, end);
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

        public Tenant Tenant => _tenant ?? throw new InvalidOperationException("Tenant is not available.");

        public bool WasUpdated { get; private set; }

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
            WasUpdated = true;
            return Task.CompletedTask;
        }

        public Task<Tenant?> GetBySlugAsync(TenantSlug slug, CancellationToken cancellationToken = default)
        {
            if (_tenant is null)
                return Task.FromResult<Tenant?>(null);
            return Task.FromResult(_tenant.Slug == slug ? _tenant : null);
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
