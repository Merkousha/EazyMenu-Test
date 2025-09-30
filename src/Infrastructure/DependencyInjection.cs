using System;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Interfaces.Provisioning;
using EazyMenu.Infrastructure.Persistence;
using EazyMenu.Infrastructure.Persistence.Repositories;
using EazyMenu.Infrastructure.Provisioning;
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
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<EazyMenuDbContext>());

        return services;
    }
}
