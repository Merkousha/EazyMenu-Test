using EazyMenu.Application.Common.Interfaces;
using EazyMenu.Application.Common.Time;
using Microsoft.Extensions.DependencyInjection;

namespace EazyMenu.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // TODO: register MediatR, validators, and mapping profiles when implemented.
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        return services;
    }
}
