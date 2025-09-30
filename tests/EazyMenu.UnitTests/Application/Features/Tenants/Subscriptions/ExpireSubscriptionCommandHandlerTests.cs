using System;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Features.Tenants.Subscriptions.ExpireSubscription;
using EazyMenu.Domain.Aggregates.Tenants;
using EazyMenu.Domain.ValueObjects;
using Xunit;

namespace EazyMenu.UnitTests.Application.Features.Tenants.Subscriptions;

public sealed class ExpireSubscriptionCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldExpireActiveSubscription()
    {
        var tenant = CreateTenantWithActiveSubscription();
        var repository = new InMemoryTenantRepository(tenant);
        var handler = new ExpireSubscriptionCommandHandler(repository, new FakeUnitOfWork());
        var activeSubscription = tenant.ActiveSubscription!;
        var expiration = activeSubscription.StartDateUtc.AddDays(10);

        var command = new ExpireSubscriptionCommand(tenant.Id.Value, expiration);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result);
        Assert.True(repository.WasUpdated);
        Assert.Null(tenant.ActiveSubscription);

        Assert.Contains(repository.Tenant.Subscriptions, subscription =>
            subscription.Id == activeSubscription.Id &&
            subscription.Status == SubscriptionStatus.Expired &&
            subscription.EndDateUtc == expiration);
    }

    [Fact]
    public async Task HandleAsync_WhenTenantNotFound_ShouldThrow()
    {
        var repository = new InMemoryTenantRepository(null);
        var handler = new ExpireSubscriptionCommandHandler(repository, new FakeUnitOfWork());
        var command = new ExpireSubscriptionCommand(Guid.NewGuid(), DateTime.UtcNow);

        await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenNoActiveSubscription_ShouldThrow()
    {
        var tenant = CreateTenant();
        var repository = new InMemoryTenantRepository(tenant);
        var handler = new ExpireSubscriptionCommandHandler(repository, new FakeUnitOfWork());
        var command = new ExpireSubscriptionCommand(tenant.Id.Value, DateTime.UtcNow);

        await Assert.ThrowsAsync<BusinessRuleViolationException>(() => handler.HandleAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenExpirationBeforeStart_ShouldThrow()
    {
        var tenant = CreateTenantWithActiveSubscription();
        var repository = new InMemoryTenantRepository(tenant);
        var handler = new ExpireSubscriptionCommandHandler(repository, new FakeUnitOfWork());
        var activeSubscription = tenant.ActiveSubscription!;
        var command = new ExpireSubscriptionCommand(tenant.Id.Value, activeSubscription.StartDateUtc.AddSeconds(-1));

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
        var subscription = Subscription.Create(SubscriptionPlan.Pro, Money.From(800000m), start, end);
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
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
