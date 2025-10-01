using EazyMenu.Application;
using EazyMenu.Infrastructure;
using EazyMenu.Infrastructure.Menus;
using EazyMenu.Infrastructure.Notifications;
using EazyMenu.Infrastructure.Persistence;
using EazyMenu.Infrastructure.Persistence.Seed;
using EazyMenu.Web.Services;
using EazyMenu.Web.Services.Orders;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration);

builder.Services.AddSignalR();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IDashboardTenantProvider, DashboardTenantProvider>();
builder.Services.AddScoped<DashboardOrderViewModelFactory>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<EazyMenuDbContext>();
    await dbContext.Database.MigrateAsync();
    await EazyMenuDbContextSeeder.SeedAsync(dbContext);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<SmsAlertsHub>("/hubs/sms-alerts");
app.MapHub<MenuUpdatesHub>("/hubs/menu-updates");

app.Run();
