using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EazyMenu.Web.Controllers;

[Authorize]
public sealed class DashboardController : Controller
{
    public IActionResult Index()
    {
        var userName = User.Identity?.Name ?? "کاربر";
        var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "نامشخص";
        var tenantId = User.FindFirst("TenantId")?.Value ?? "نامشخص";

        ViewBag.UserName = userName;
        ViewBag.UserRole = userRole;
        ViewBag.TenantId = tenantId;

        return View();
    }
}
