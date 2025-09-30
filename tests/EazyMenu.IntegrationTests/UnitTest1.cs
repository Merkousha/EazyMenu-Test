using System;
using System.Threading.Tasks;
using EazyMenu.Application;
using EazyMenu.Application.Common.Interfaces.Provisioning;
using EazyMenu.Application.Features.Onboarding.RegisterTenant;
using EazyMenu.Domain.Aggregates.Tenants;
using EazyMenu.Infrastructure;
using EazyMenu.Infrastructure.Persistence;
using EazyMenu.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EazyMenu.IntegrationTests;

public class RegisterTenantFlowTests
{
    [Fact]
    public async Task ProvisionTenant_ReturnsNonEmptyTenantId()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplicationServices();

        var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
        services.AddInfrastructureServices(configuration, options =>
        {
            options.UseInMemoryDatabase($"RegisterTenant-{Guid.NewGuid()}");
        });

        await using var provider = services.BuildServiceProvider();
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

        var tenantId = await handler.HandleAsync(command);

        Assert.NotEqual(Guid.Empty, tenantId.Value);

        var dbContext = scope.ServiceProvider.GetRequiredService<EazyMenuDbContext>();
        var tenant = await dbContext.Tenants
            .Include(t => t.Subscriptions)
            .Include(t => t.ActiveSubscription)
            .FirstOrDefaultAsync(t => t.Id == tenantId);

        Assert.NotNull(tenant);
        Assert.NotNull(tenant!.ActiveSubscription);
        Assert.Equal(SubscriptionPlan.Starter, tenant.ActiveSubscription!.Plan);
        Assert.True(tenant.ActiveSubscription.IsTrial);
        Assert.Single(tenant.Subscriptions);

        var provisioningRecord = await dbContext.TenantProvisionings.FirstOrDefaultAsync(r => r.TenantId == tenantId.Value);
        Assert.NotNull(provisioningRecord);
        Assert.Equal("starter", provisioningRecord!.PlanCode);
        Assert.True(provisioningRecord.UseTrial);
    }
}
