using System;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Interfaces.Menus;
using EazyMenu.Application.Common.Interfaces.Identity;
using EazyMenu.Application.Common.Interfaces.Notifications;
using EazyMenu.Application.Common.Interfaces.Orders;
using EazyMenu.Application.Common.Interfaces.Payments;
using EazyMenu.Application.Common.Interfaces.Pricing;
using EazyMenu.Application.Common.Interfaces.Provisioning;
using EazyMenu.Application.Common.Interfaces.Security;
using EazyMenu.Infrastructure.Menus;
using EazyMenu.Infrastructure.Identity;
using EazyMenu.Infrastructure.Persistence;
using EazyMenu.Infrastructure.Persistence.Repositories;
using EazyMenu.Infrastructure.Payments;
using EazyMenu.Infrastructure.Pricing;
using EazyMenu.Infrastructure.Provisioning;
using EazyMenu.Infrastructure.Notifications;
using EazyMenu.Infrastructure.Security;
using EazyMenu.Infrastructure.Services.Orders;
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
        services.AddScoped<EfSmsDeliveryStore>();
        services.AddScoped<ISmsDeliveryStore>(sp => sp.GetRequiredService<EfSmsDeliveryStore>());
        services.AddScoped<ISmsDeliveryLogReader>(sp => sp.GetRequiredService<EfSmsDeliveryStore>());
        services.AddScoped<ISmsUsageReader, EfSmsUsageReader>();
        services.AddSingleton(provider =>
        {
            var options = new EmailOptions();
            var section = configuration.GetSection("Notifications:Email");
            if (section is not null)
            {
                options.FromAddress = section[nameof(EmailOptions.FromAddress)] ?? options.FromAddress;
                options.SupportAddress = section[nameof(EmailOptions.SupportAddress)] ?? options.SupportAddress;
            }

            return options;
        });
        services.AddSingleton(provider =>
        {
            var options = new SmsOptions();
            var section = configuration.GetSection("Notifications:Sms");
            if (section is not null)
            {
                options.Provider = section["Provider"] ?? options.Provider;
                options.KavenegarApiKey = section["KavenegarApiKey"];
                options.KavenegarSenderLine = section["KavenegarSenderLine"];
            }

            return options;
        });
        services.AddScoped<LoggingSmsSender>();
        services.AddHttpClient<KavenegarSmsSender>(client =>
        {
            client.BaseAddress = new Uri("https://api.kavenegar.com/");
            client.Timeout = TimeSpan.FromSeconds(10);
        });
        services.AddScoped<IEmailSender, LoggingEmailSender>();
        services.AddScoped<ISmsFailureAlertNotifier, SignalRSmsFailureAlertNotifier>();
        services.AddScoped<IMenuRealtimeNotifier, SignalRMenuRealtimeNotifier>();
        services.AddScoped<IOrderRealtimeNotifier, SignalROrderNotifier>();
        services.AddScoped<EfMenuPublicationStore>();
    services.AddScoped<IMenuPublicationWriter>(sp => sp.GetRequiredService<EfMenuPublicationStore>());
    services.AddScoped<IMenuPublicationReader>(sp => sp.GetRequiredService<EfMenuPublicationStore>());
        services.AddScoped<ISmsFailureAlertService, SmsFailureAlertService>();
        services.AddScoped<ISmsSender>(provider =>
        {
            var smsOptions = provider.GetRequiredService<SmsOptions>();
            return smsOptions.GetProvider() switch
            {
                SmsProvider.Kavenegar => provider.GetRequiredService<KavenegarSmsSender>(),
                _ => provider.GetRequiredService<LoggingSmsSender>()
            };
        });
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

    services.AddScoped<IMenuRepository, MenuRepository>();
    services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderNumberGenerator, SequentialOrderNumberGenerator>();
        
        // Identity services
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IUserRepository, UserRepository>();
        
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<EazyMenuDbContext>());

        return services;
    }
}
