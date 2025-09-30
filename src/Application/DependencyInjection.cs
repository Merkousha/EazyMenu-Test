using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Common.Interfaces;
using EazyMenu.Application.Common.Time;
using EazyMenu.Application.Common.Interfaces.Provisioning;
using EazyMenu.Application.Features.Onboarding.RegisterTenant;
using EazyMenu.Application.Features.Payments.VerifyPayment;
using Microsoft.Extensions.DependencyInjection;

namespace EazyMenu.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // TODO: register MediatR, validators, and mapping profiles when implemented.
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddScoped<ICommandHandler<RegisterTenantCommand, TenantProvisioningResult>, RegisterTenantCommandHandler>();
        services.AddScoped<ICommandHandler<VerifyPaymentCommand, VerifyPaymentResult>, VerifyPaymentCommandHandler>();
        return services;
    }
}
