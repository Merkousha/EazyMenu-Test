using System;
using EazyMenu.Application;
using EazyMenu.Infrastructure;
using EazyMenu.Infrastructure.Persistence;
using EazyMenu.Infrastructure.Persistence.Seed;
using EazyMenu.Public.Options;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration);

builder.Services.Configure<TenantSiteOptions>(builder.Configuration.GetSection("TenantSite"));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/auth/login";
        options.LogoutPath = "/auth/logout";
        options.Cookie.Name = "EazyMenu.Customer";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

builder.Services.AddControllersWithViews();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
