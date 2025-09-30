using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Common.Interfaces;
using EazyMenu.Application.Common.Time;
using EazyMenu.Application.Common.Interfaces.Provisioning;
using EazyMenu.Application.Common.Notifications;
using EazyMenu.Application.Features.Customers.Login;
using EazyMenu.Application.Features.Onboarding.RegisterTenant;
using EazyMenu.Application.Features.Notifications.GetSmsDeliveryLogs;
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
    services.AddScoped<ICommandHandler<RequestCustomerLoginCommand, RequestCustomerLoginResult>, RequestCustomerLoginCommandHandler>();
    services.AddScoped<ICommandHandler<VerifyCustomerLoginCommand, VerifyCustomerLoginResult>, VerifyCustomerLoginCommandHandler>();
        services.AddScoped<ICommandHandler<VerifyPaymentCommand, VerifyPaymentResult>, VerifyPaymentCommandHandler>();
        services.AddScoped<IQueryHandler<GetSmsDeliveryLogsQuery, SmsDeliveryLogPage>, GetSmsDeliveryLogsQueryHandler>();
        return services;
    }
}
