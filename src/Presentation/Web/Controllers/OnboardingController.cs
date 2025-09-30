using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Common.Interfaces.Provisioning;
using EazyMenu.Application.Features.Onboarding.RegisterTenant;
using EazyMenu.Domain.Aggregates.Tenants;
using EazyMenu.Domain.Common.Exceptions;
using EazyMenu.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EazyMenu.Web.Controllers;

[Route("onboarding")]
public sealed class OnboardingController : Controller
{
    private readonly ICommandHandler<RegisterTenantCommand, TenantProvisioningResult> _registerTenantCommandHandler;
    private readonly ILogger<OnboardingController> _logger;

    public OnboardingController(
        ICommandHandler<RegisterTenantCommand, TenantProvisioningResult> registerTenantCommandHandler,
        ILogger<OnboardingController> logger)
    {
        _registerTenantCommandHandler = registerTenantCommandHandler;
        _logger = logger;
    }

    [HttpGet("start")]
    public IActionResult Start()
    {
        var model = new RegisterTenantViewModel
        {
            PlanCode = SubscriptionPlan.Starter.ToString().ToLowerInvariant(),
            Plans = BuildPlanOptions(SubscriptionPlan.Starter.ToString().ToLowerInvariant()),
            UseTrial = true
        };

        ViewData["Title"] = "آغاز ثبت‌نام";
        return View(model);
    }

    [HttpPost("start")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(RegisterTenantViewModel model, CancellationToken cancellationToken)
    {
        model.PlanCode = (model.PlanCode ?? string.Empty).Trim().ToLowerInvariant();
        model.DiscountCode = string.IsNullOrWhiteSpace(model.DiscountCode)
            ? null
            : model.DiscountCode.Trim();
        model.Plans = BuildPlanOptions(model.PlanCode);

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "آغاز ثبت‌نام";
            return View(model);
        }

        var command = new RegisterTenantCommand(
            model.RestaurantName.Trim(),
            model.ManagerEmail.Trim(),
            model.ManagerPhone.Trim(),
            model.PlanCode,
            model.City.Trim(),
            model.Street.Trim(),
            model.PostalCode.Trim(),
            model.UseTrial,
            model.DiscountCode);

        try
        {
            var result = await _registerTenantCommandHandler.HandleAsync(command, cancellationToken);

            if (result.Payment is not null)
            {
                if (result.Payment.RedirectUri is not null)
                {
                    TempData["OnboardingTenantId"] = result.TenantId.Value.ToString();
                    TempData["OnboardingSubscriptionId"] = result.SubscriptionId.ToString();
                    return Redirect(result.Payment.RedirectUri.AbsoluteUri);
                }

                var paymentFallback = new ProvisioningSuccessViewModel
                {
                    Title = "ادامه پرداخت مورد نیاز است",
                    Message = "لینک پرداخت در حال حاضر در دسترس نیست. لطفاً با پشتیبانی تماس بگیرید.",
                    TenantId = result.TenantId.Value.ToString(),
                    SubscriptionId = result.SubscriptionId.ToString()
                };

                ViewData["Title"] = paymentFallback.Title;
                return View("Success", paymentFallback);
            }

            var successModel = new ProvisioningSuccessViewModel
            {
                Title = "ثبت‌نام شما تکمیل شد",
                Message = "حساب کاربری و اشتراک شما فعال شد. جزئیات ورود به‌زودی برایتان ارسال می‌شود.",
                TenantId = result.TenantId.Value.ToString(),
                SubscriptionId = result.SubscriptionId.ToString()
            };

            ViewData["Title"] = successModel.Title;
            return View("Success", successModel);
        }
        catch (BusinessRuleViolationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation during onboarding");
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain validation failed during onboarding");
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during onboarding");
            ModelState.AddModelError(string.Empty, "در فرایند ثبت‌نام خطایی رخ داد. لطفاً مجدداً تلاش کنید.");
        }

        ViewData["Title"] = "آغاز ثبت‌نام";
        return View(model);
    }

    private static IReadOnlyCollection<SelectListItem> BuildPlanOptions(string? selectedPlan)
    {
        var options = new List<(SubscriptionPlan Plan, string Title, string Description)>
        {
            (SubscriptionPlan.Starter, "پلن استارتر", "مناسب کافه‌های تازه‌کار با امکانات پایه"),
            (SubscriptionPlan.Pro, "پلن پرو", "امکانات کامل مدیریت منو و سفارش آنلاین"),
            (SubscriptionPlan.Enterprise, "پلن سازمانی", "سفارشی‌سازی برای زنجیره رستورانی و برندهای بزرگ")
        };

        var normalizedSelected = (selectedPlan ?? SubscriptionPlan.Starter.ToString().ToLowerInvariant()).Trim().ToLowerInvariant();

        return options
            .Select(option => new SelectListItem
            {
                Value = option.Plan.ToString().ToLowerInvariant(),
                Text = option.Title,
                Selected = string.Equals(option.Plan.ToString().ToLowerInvariant(), normalizedSelected, StringComparison.OrdinalIgnoreCase),
                Group = new SelectListGroup { Name = option.Description }
            })
            .ToArray();
    }
}
