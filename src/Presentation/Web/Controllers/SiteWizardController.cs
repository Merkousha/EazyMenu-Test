using EazyMenu.Application.Features.Tenants.Commands.UpdateBranding;
using EazyMenu.Application.Features.Tenants.Queries.GetTenantBranding;
using EazyMenu.Web.Extensions;
using EazyMenu.Web.Models.SiteWizard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EazyMenu.Web.Controllers;

[Authorize]
[Route("site-wizard")]
public sealed class SiteWizardController : Controller
{
    private readonly UpdateBrandingHandler _updateBrandingHandler;
    private readonly GetTenantBrandingHandler _getTenantBrandingHandler;
    private readonly ILogger<SiteWizardController> _logger;

    public SiteWizardController(
        UpdateBrandingHandler updateBrandingHandler,
        GetTenantBrandingHandler getTenantBrandingHandler,
        ILogger<SiteWizardController> logger)
    {
        _updateBrandingHandler = updateBrandingHandler;
        _getTenantBrandingHandler = getTenantBrandingHandler;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var tenantId = User.GetTenantId();
        if (!tenantId.HasValue)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        var branding = await _getTenantBrandingHandler.HandleAsync(
            new GetTenantBrandingQuery { TenantId = tenantId.Value },
            cancellationToken);

        var model = new SiteWizardViewModel
        {
            DisplayName = branding?.DisplayName ?? string.Empty,
            LogoUrl = branding?.LogoUrl,
            PrimaryColor = branding?.PrimaryColor ?? "#FF6B35",
            SecondaryColor = branding?.SecondaryColor ?? "#004E89",
            BannerImageUrl = branding?.BannerImageUrl,
            AboutText = branding?.AboutText,
            OpeningHours = branding?.OpeningHours,
            TemplateName = branding?.TemplateName ?? "classic",
            IsPublished = branding?.IsPublished ?? false
        };

        ViewData["Title"] = "ویزارد سایت‌ساز";
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateBranding(SiteWizardViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "ویزارد سایت‌ساز";
            return View("Index", model);
        }

        var tenantId = User.GetTenantId();
        if (!tenantId.HasValue)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        var command = new UpdateBrandingCommand
        {
            TenantId = tenantId.Value,
            DisplayName = model.DisplayName,
            LogoUrl = model.LogoUrl,
            PrimaryColor = model.PrimaryColor,
            SecondaryColor = model.SecondaryColor,
            BannerImageUrl = model.BannerImageUrl,
            AboutText = model.AboutText,
            OpeningHours = model.OpeningHours,
            TemplateName = model.TemplateName,
            ShouldPublish = model.ShouldPublish
        };

        try
        {
            var result = await _updateBrandingHandler.HandleAsync(command, cancellationToken);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Error ?? "خطای ناشناخته");
                ViewData["Title"] = "ویزارد سایت‌ساز";
                return View("Index", model);
            }

            TempData["SuccessMessage"] = "تنظیمات سایت با موفقیت ذخیره شد";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating branding for tenant {TenantId}", tenantId);
            ModelState.AddModelError(string.Empty, "خطا در ذخیره‌سازی تنظیمات");
            ViewData["Title"] = "ویزارد سایت‌ساز";
            return View("Index", model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(CancellationToken cancellationToken)
    {
        var tenantId = User.GetTenantId();
        if (!tenantId.HasValue)
        {
            return Json(new { success = false, error = "شناسه رستوران یافت نشد" });
        }

        var branding = await _getTenantBrandingHandler.HandleAsync(
            new GetTenantBrandingQuery { TenantId = tenantId.Value },
            cancellationToken);

        if (branding == null)
        {
            return Json(new { success = false, error = "تنظیمات سایت یافت نشد" });
        }

        var command = new UpdateBrandingCommand
        {
            TenantId = tenantId.Value,
            DisplayName = branding.DisplayName,
            LogoUrl = branding.LogoUrl,
            PrimaryColor = branding.PrimaryColor,
            SecondaryColor = branding.SecondaryColor,
            BannerImageUrl = branding.BannerImageUrl,
            AboutText = branding.AboutText,
            OpeningHours = branding.OpeningHours,
            TemplateName = branding.TemplateName,
            ShouldPublish = true
        };

        try
        {
            var result = await _updateBrandingHandler.HandleAsync(command, cancellationToken);

            if (!result.IsSuccess)
            {
                return Json(new { success = false, error = result.Error });
            }

            return Json(new { success = true, message = "سایت با موفقیت منتشر شد" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing site for tenant {TenantId}", tenantId);
            return Json(new { success = false, error = "خطا در انتشار سایت" });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unpublish(CancellationToken cancellationToken)
    {
        var tenantId = User.GetTenantId();
        if (!tenantId.HasValue)
        {
            return Json(new { success = false, error = "شناسه رستوران یافت نشد" });
        }

        var branding = await _getTenantBrandingHandler.HandleAsync(
            new GetTenantBrandingQuery { TenantId = tenantId.Value },
            cancellationToken);

        if (branding == null)
        {
            return Json(new { success = false, error = "تنظیمات سایت یافت نشد" });
        }

        var command = new UpdateBrandingCommand
        {
            TenantId = tenantId.Value,
            DisplayName = branding.DisplayName,
            LogoUrl = branding.LogoUrl,
            PrimaryColor = branding.PrimaryColor,
            SecondaryColor = branding.SecondaryColor,
            BannerImageUrl = branding.BannerImageUrl,
            AboutText = branding.AboutText,
            OpeningHours = branding.OpeningHours,
            TemplateName = branding.TemplateName,
            ShouldPublish = false
        };

        try
        {
            var result = await _updateBrandingHandler.HandleAsync(command, cancellationToken);

            if (!result.IsSuccess)
            {
                return Json(new { success = false, error = result.Error });
            }

            return Json(new { success = true, message = "سایت با موفقیت مخفی شد" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unpublishing site for tenant {TenantId}", tenantId);
            return Json(new { success = false, error = "خطا در مخفی‌سازی سایت" });
        }
    }
}
