using System;
using System.Threading.Tasks;
using EazyMenu.Application.Features.Onboarding.RegisterTenant;
using EazyMenu.Infrastructure.Provisioning;
using Xunit;

namespace EazyMenu.IntegrationTests;

public class RegisterTenantFlowTests
{
    [Fact]
    public async Task ProvisionTenant_ReturnsNonEmptyTenantId()
    {
        var provisioningService = new InMemoryTenantProvisioningService();
        var handler = new RegisterTenantCommandHandler(provisioningService);

        var tenantId = await handler.HandleAsync(new RegisterTenantCommand(
            "Cafe Azadi",
            "owner@eazymenu.ir",
            "starter"));

        Assert.NotEqual(Guid.Empty, tenantId.Value);
    }
}
