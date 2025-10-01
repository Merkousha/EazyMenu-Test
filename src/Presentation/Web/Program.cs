using EazyMenu.Application;
using EazyMenu.Infrastructure;
using EazyMenu.Infrastructure.Extensions;
using EazyMenu.Infrastructure.Menus;
using EazyMenu.Infrastructure.Notifications;
using EazyMenu.Infrastructure.Persistence;
using EazyMenu.Infrastructure.Persistence.Seed;
using EazyMenu.Web.Services;
using EazyMenu.Web.Services.Orders;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration);

// Configure Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(12);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

// Configure Authorization Policies
builder.Services.AddAuthorization(options =>
{
    // Owner policy - full access
    options.AddPolicy("OwnerOnly", policy => 
        policy.RequireRole("Owner"));
    
    // Manager policy - most features (Owner + Manager)
    options.AddPolicy("ManagerAccess", policy => 
        policy.RequireRole("Owner", "Manager"));
    
    // Staff policy - limited access (Owner + Manager + Staff)
    options.AddPolicy("StaffAccess", policy => 
        policy.RequireRole("Owner", "Manager", "Staff"));
});

builder.Services.AddSignalR();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IDashboardTenantProvider, DashboardTenantProvider>();
builder.Services.AddScoped<DashboardOrderViewModelFactory>();

var app = builder.Build();

// Initialize Database و Seed Data در محیط Development
if (app.Environment.IsDevelopment())
{
    var seedData = builder.Configuration.GetValue<bool>("SeedData", true);
    await app.InitializeDatabaseAsync(seedData);
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<SmsAlertsHub>("/hubs/sms-alerts");
app.MapHub<MenuUpdatesHub>("/hubs/menu-updates");
app.MapHub<OrderAlertsHub>("/hubs/order-alerts");

app.Run();
