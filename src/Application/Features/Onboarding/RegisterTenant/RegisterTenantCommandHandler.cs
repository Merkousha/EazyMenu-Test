using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Common.Interfaces.Provisioning;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Onboarding.RegisterTenant;

public sealed class RegisterTenantCommandHandler : ICommandHandler<RegisterTenantCommand, TenantProvisioningResult>
{
    private readonly ITenantProvisioningService _tenantProvisioningService;

    public RegisterTenantCommandHandler(ITenantProvisioningService tenantProvisioningService)
    {
        _tenantProvisioningService = tenantProvisioningService;
    }

    public Task<TenantProvisioningResult> HandleAsync(RegisterTenantCommand command, CancellationToken cancellationToken = default)
    {
        var address = Address.Create(command.City, command.Street, command.PostalCode);
        var request = new TenantProvisioningRequest(
            command.RestaurantName,
            command.ManagerEmail,
            command.ManagerPhone,
            command.PlanCode,
            address,
            command.UseTrial,
            command.DiscountCode);

        return _tenantProvisioningService.ProvisionAsync(request, cancellationToken);
    }
}
