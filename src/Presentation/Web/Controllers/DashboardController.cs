using System;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Features.Tenants.Common;
using EazyMenu.Application.Features.Tenants.Subscriptions.GetActiveSubscription;
using EazyMenu.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EazyMenu.Web.Controllers;

[Authorize]
public sealed class DashboardController : Controller
{
    private readonly IQueryHandler<GetActiveSubscriptionQuery, SubscriptionDetailsDto?> _getActiveSubscriptionHandler;

    public DashboardController(IQueryHandler<GetActiveSubscriptionQuery, SubscriptionDetailsDto?> getActiveSubscriptionHandler)
    {
        _getActiveSubscriptionHandler = getActiveSubscriptionHandler;
    }

    public async Task<IActionResult> Index()
    {
        var userName = User.Identity?.Name ?? "کاربر";
        var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "نامشخص";
        var tenantId = User.GetTenantId();

        ViewBag.UserName = userName;
        ViewBag.UserRole = userRole;
        ViewBag.TenantId = tenantId?.ToString() ?? "نامشخص";

        // Get active subscription details
        if (tenantId.HasValue)
        {
            try
            {
                var subscription = await _getActiveSubscriptionHandler.HandleAsync(
                    new GetActiveSubscriptionQuery(tenantId.Value));
                ViewBag.Subscription = subscription;
            }
            catch
            {
                // If query fails, subscription will be null and UI will show appropriate message
                ViewBag.Subscription = null;
            }
        }

        return View();
    }
}
