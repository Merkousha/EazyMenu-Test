using System;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Features.Tenants.Subscriptions.ReinstateSubscription;
using EazyMenu.Application.Features.Tenants.Subscriptions.SuspendSubscription;
using EazyMenu.Domain.Aggregates.Tenants;
using EazyMenu.Domain.ValueObjects;
using Xunit;

namespace EazyMenu.UnitTests.Application.Features.Tenants.Subscriptions;

public sealed class SuspendAndReinstateSubscriptionCommandHandlerTests
{
    [Fact]
    public async Task Suspend_ShouldChangeStatusToSuspended()
    {
        var tenant = CreateTenantWithActiveSubscription();
        var repository = new InMemoryTenantRepository(tenant);
        var handler = new SuspendSubscriptionCommandHandler(repository, new FakeUnitOfWork());
        var command = new SuspendSubscriptionCommand(tenant.Id.Value);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result);
        Assert.True(repository.WasUpdated);
        Assert.NotNull(tenant.ActiveSubscription);
        Assert.Equal(SubscriptionStatus.Suspended, tenant.ActiveSubscription!.Status);
    }

    [Fact]
    public async Task Suspend_WhenTenantNotFound_ShouldThrow()
    {
        var repository = new InMemoryTenantRepository(null);
        var handler = new SuspendSubscriptionCommandHandler(repository, new FakeUnitOfWork());
        var command = new SuspendSubscriptionCommand(Guid.NewGuid());

        await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task Suspend_WhenNoActiveSubscription_ShouldThrow()
    {
        var tenant = CreateTenant();
        var repository = new InMemoryTenantRepository(tenant);
        var handler = new SuspendSubscriptionCommandHandler(repository, new FakeUnitOfWork());
        var command = new SuspendSubscriptionCommand(tenant.Id.Value);

        await Assert.ThrowsAsync<BusinessRuleViolationException>(() => handler.HandleAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task Reinstate_ShouldActivateSuspendedSubscription()
    {
        var tenant = CreateTenantWithActiveSubscription();
        tenant.SuspendActiveSubscription();
        var repository = new InMemoryTenantRepository(tenant);
        var handler = new ReinstateSubscriptionCommandHandler(repository, new FakeUnitOfWork());
        var command = new ReinstateSubscriptionCommand(tenant.Id.Value);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result);
        Assert.True(repository.WasUpdated);
        Assert.NotNull(tenant.ActiveSubscription);
        Assert.Equal(SubscriptionStatus.Active, tenant.ActiveSubscription!.Status);
    }

    [Fact]
    public async Task Reinstate_WhenTenantNotFound_ShouldThrow()
    {
        var repository = new InMemoryTenantRepository(null);
        var handler = new ReinstateSubscriptionCommandHandler(repository, new FakeUnitOfWork());
        var command = new ReinstateSubscriptionCommand(Guid.NewGuid());

        await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task Reinstate_WhenNoActiveSubscription_ShouldThrow()
    {
        var tenant = CreateTenant();
        var repository = new InMemoryTenantRepository(tenant);
        var handler = new ReinstateSubscriptionCommandHandler(repository, new FakeUnitOfWork());
        var command = new ReinstateSubscriptionCommand(tenant.Id.Value);

        await Assert.ThrowsAsync<BusinessRuleViolationException>(() => handler.HandleAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task Reinstate_WhenSubscriptionNotSuspended_ShouldThrow()
    {
        var tenant = CreateTenantWithActiveSubscription();
        var repository = new InMemoryTenantRepository(tenant);
        var handler = new ReinstateSubscriptionCommandHandler(repository, new FakeUnitOfWork());
        var command = new ReinstateSubscriptionCommand(tenant.Id.Value);

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
        var subscription = Subscription.Create(SubscriptionPlan.Pro, Money.From(600000m), start, end);
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
