using EazyMenu.Infrastructure.Persistence;
using EazyMenu.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EazyMenu.Infrastructure.Extensions;

public static class DatabaseExtensions
{
    /// <summary>
    /// اعمال Migrations و Seed کردن داده‌های اولیه
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IHost host, bool seedData = false)
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<EazyMenuDbContext>>();

        try
        {
            var context = services.GetRequiredService<EazyMenuDbContext>();

            logger.LogInformation("شروع Migration دیتابیس...");
            await context.Database.MigrateAsync();
            logger.LogInformation("✅ Migration دیتابیس با موفقیت انجام شد");

            if (seedData)
            {
                logger.LogInformation("شروع Seed کردن داده‌های نمونه...");
                var seeder = new DevelopmentDataSeeder(context, services.GetRequiredService<ILogger<DevelopmentDataSeeder>>());
                await seeder.SeedAsync();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ خطا در Initialize کردن دیتابیس");
            throw;
        }
    }
}
