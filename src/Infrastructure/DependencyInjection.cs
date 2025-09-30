using System;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Interfaces.Payments;
using EazyMenu.Application.Common.Interfaces.Pricing;
using EazyMenu.Application.Common.Interfaces.Provisioning;
using EazyMenu.Application.Common.Interfaces.Notifications;
using EazyMenu.Application.Common.Interfaces.Security;
using EazyMenu.Infrastructure.Persistence;
using EazyMenu.Infrastructure.Persistence.Repositories;
using EazyMenu.Infrastructure.Payments;
using EazyMenu.Infrastructure.Pricing;
using EazyMenu.Infrastructure.Provisioning;
using EazyMenu.Infrastructure.Notifications;
using EazyMenu.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace EazyMenu.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DbContextOptionsBuilder>? configureDbContext = null)
    {
        services.AddScoped<ITenantProvisioningService, EfTenantProvisioningService>();
        services.AddScoped<ISubscriptionPricingService, SubscriptionPricingService>();
        services.AddScoped<IPaymentGatewayClient, ZarinpalSandboxPaymentGatewayClient>();
        services.AddSingleton<IOneTimePasswordGenerator, RandomOneTimePasswordGenerator>();
        services.AddScoped<IOneTimePasswordStore, InMemoryOneTimePasswordStore>();
        services.AddSingleton<ISmsSender, LoggingSmsSender>();
        services.AddMemoryCache();
        services.AddSingleton(provider =>
        {
            var options = new PaymentGatewayOptions();
            var section = configuration.GetSection("Payments");
            var callback = section?["CallbackUri"];
            if (!string.IsNullOrWhiteSpace(callback))
            {
                options.CallbackUri = callback!;
            }

            return options;
        });

        if (configureDbContext is not null)
        {
            services.AddDbContext<EazyMenuDbContext>(configureDbContext);
        }
        else
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("تنظیم اتصال پایگاه‌داده (DefaultConnection) در فایل پیکربندی یافت نشد.");
            }

            services.AddDbContext<EazyMenuDbContext>(options =>
            {
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(EazyMenuDbContext).Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure();
                });
            });
        }

        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<EazyMenuDbContext>());

        return services;
    }
}
