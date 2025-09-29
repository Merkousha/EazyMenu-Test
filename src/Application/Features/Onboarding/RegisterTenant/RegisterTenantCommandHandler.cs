using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Common.Interfaces.Provisioning;
using EazyMenu.Domain.Entities;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Onboarding.RegisterTenant;

public sealed class RegisterTenantCommandHandler : ICommandHandler<RegisterTenantCommand, TenantId>
{
    private readonly ITenantProvisioningService _tenantProvisioningService;

    public RegisterTenantCommandHandler(ITenantProvisioningService tenantProvisioningService)
    {
        _tenantProvisioningService = tenantProvisioningService;
    }

    public Task<TenantId> HandleAsync(RegisterTenantCommand command, CancellationToken cancellationToken = default)
    {
        var restaurant = new Restaurant(TenantId.New(), command.RestaurantName, Slugify(command.RestaurantName));
        return _tenantProvisioningService.ProvisionAsync(restaurant, cancellationToken);
    }

    private static string Slugify(string value)
    {
        return value
            .Trim()
            .ToLowerInvariant()
            .Replace(' ', '-');
    }
}
