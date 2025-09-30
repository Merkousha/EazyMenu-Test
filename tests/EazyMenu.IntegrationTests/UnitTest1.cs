using System;
using System.Threading.Tasks;
using EazyMenu.Application;
using EazyMenu.Application.Common.Interfaces.Provisioning;
using EazyMenu.Application.Features.Onboarding.RegisterTenant;
using EazyMenu.Domain.Aggregates.Payments;
using EazyMenu.Domain.Aggregates.Tenants;
using EazyMenu.Domain.ValueObjects;
using EazyMenu.Infrastructure;
using EazyMenu.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EazyMenu.IntegrationTests;

public class RegisterTenantFlowTests
{
    [Fact]
    public async Task ProvisionTenant_WithTrial_ReturnsActiveSubscriptionWithoutPayment()
    {
        await using var provider = BuildServiceProvider();
        using var scope = provider.CreateScope();

        var provisioningService = scope.ServiceProvider.GetRequiredService<ITenantProvisioningService>();
        var handler = new RegisterTenantCommandHandler(provisioningService);

        var command = new RegisterTenantCommand(
            "Cafe Azadi",
            "owner@eazymenu.ir",
            "+989121234567",
            "starter",
            "Tehran",
            "Valiasr Ave",
            "1966731111",
            UseTrial: true,
            DiscountCode: "WELCOME10");

        var result = await handler.HandleAsync(command);

        Assert.NotEqual(Guid.Empty, result.TenantId.Value);
        Assert.NotEqual(Guid.Empty, result.SubscriptionId);
        Assert.Null(result.Payment);

        var dbContext = scope.ServiceProvider.GetRequiredService<EazyMenuDbContext>();
        var tenant = await dbContext.Tenants
            .Include(t => t.Subscriptions)
            .Include(t => t.ActiveSubscription)
            .FirstOrDefaultAsync(t => t.Id == result.TenantId);

        Assert.NotNull(tenant);
        Assert.NotNull(tenant!.ActiveSubscription);
        Assert.Equal(SubscriptionPlan.Starter, tenant.ActiveSubscription!.Plan);
        Assert.True(tenant.ActiveSubscription.IsTrial);
        Assert.Equal(SubscriptionStatus.Active, tenant.ActiveSubscription.Status);
        Assert.Single(tenant.Subscriptions);

        var provisioningRecord = await dbContext.TenantProvisionings.SingleAsync(r => r.TenantId == result.TenantId.Value);
        Assert.Equal(result.SubscriptionId, provisioningRecord.SubscriptionId);
        Assert.Null(provisioningRecord.PaymentId);
        Assert.True(provisioningRecord.UseTrial);

        var paymentCount = await dbContext.PaymentTransactions.CountAsync();
        Assert.Equal(0, paymentCount);
    }

    [Fact]
    public async Task ProvisionTenant_PaidPlan_CreatesPendingPayment()
    {
        await using var provider = BuildServiceProvider();
        using var scope = provider.CreateScope();

        var provisioningService = scope.ServiceProvider.GetRequiredService<ITenantProvisioningService>();
        var handler = new RegisterTenantCommandHandler(provisioningService);

        var command = new RegisterTenantCommand(
            "Bistro Tehran",
            "billing@eazymenu.ir",
            "+989301112233",
            "starter",
            "Tehran",
            "Enghelab St",
            "1966732222",
            UseTrial: false,
            DiscountCode: "WELCOME10");

        var result = await handler.HandleAsync(command);

        Assert.NotNull(result.Payment);
        Assert.Equal(PaymentStatus.Pending, result.Payment!.Status);
        Assert.NotNull(result.Payment.RedirectUri);
        Assert.False(string.IsNullOrWhiteSpace(result.Payment.GatewayAuthority));

        var dbContext = scope.ServiceProvider.GetRequiredService<EazyMenuDbContext>();
        var tenant = await dbContext.Tenants
            .Include(t => t.Subscriptions)
            .Include(t => t.ActiveSubscription)
            .FirstOrDefaultAsync(t => t.Id == result.TenantId);

        Assert.NotNull(tenant);
        Assert.Null(tenant!.ActiveSubscription);
        var pendingSubscription = Assert.Single(tenant.Subscriptions);
        Assert.Equal(result.SubscriptionId, pendingSubscription.Id);
        Assert.Equal(SubscriptionStatus.Pending, pendingSubscription.Status);
        Assert.False(pendingSubscription.IsTrial);

        var payment = await dbContext.PaymentTransactions.SingleAsync(p => p.Id == result.Payment.PaymentId);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.Equal(PaymentMethod.Zarinpal, payment.Method);
        Assert.Equal(891_000m, payment.Amount.Amount);
        Assert.Equal(990_000m, payment.OriginalAmount.Amount);
        Assert.Equal("IRR", payment.Amount.Currency);
        Assert.NotNull(payment.GatewayAuthority);

        var provisioningRecord = await dbContext.TenantProvisionings.SingleAsync(r => r.TenantId == result.TenantId.Value);
        Assert.Equal(result.SubscriptionId, provisioningRecord.SubscriptionId);
        Assert.Equal(payment.Id.Value, provisioningRecord.PaymentId);
        Assert.False(provisioningRecord.UseTrial);
    }

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplicationServices();

        var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
        services.AddInfrastructureServices(configuration, options =>
        {
            options.UseInMemoryDatabase($"RegisterTenant-{Guid.NewGuid()}");
        });

        return services.BuildServiceProvider();
    }
}
