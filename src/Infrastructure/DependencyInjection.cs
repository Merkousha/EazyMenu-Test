using EazyMenu.Application.Common.Interfaces.Provisioning;
using EazyMenu.Infrastructure.Provisioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EazyMenu.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ITenantProvisioningService, InMemoryTenantProvisioningService>();

        // TODO: register EF Core DbContext, Identity, payment gateways, SMS providers, etc.

        return services;
    }
}
