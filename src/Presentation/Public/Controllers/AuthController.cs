using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Features.Customers.Login;
using EazyMenu.Public.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using EazyMenu.Public.Options;

namespace EazyMenu.Public.Controllers;

[Route("auth")]
public sealed class AuthController : Controller
{
    private readonly ICommandHandler<RequestCustomerLoginCommand, RequestCustomerLoginResult> _requestLoginHandler;
    private readonly ICommandHandler<VerifyCustomerLoginCommand, VerifyCustomerLoginResult> _verifyLoginHandler;
    private readonly ILogger<AuthController> _logger;
    private readonly TenantSiteOptions _tenantOptions;

    public AuthController(
        ICommandHandler<RequestCustomerLoginCommand, RequestCustomerLoginResult> requestLoginHandler,
        ICommandHandler<VerifyCustomerLoginCommand, VerifyCustomerLoginResult> verifyLoginHandler,
        ILogger<AuthController> logger,
        IOptions<TenantSiteOptions> tenantOptions)
    {
        _requestLoginHandler = requestLoginHandler;
        _verifyLoginHandler = verifyLoginHandler;
        _logger = logger;
        _tenantOptions = tenantOptions.Value;
    }

    [HttpGet("login")]
    [AllowAnonymous]
    public IActionResult Login(string? phone = null, Guid? tenantId = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        var resolvedTenantId = ResolveTenantId(tenantId);
        var model = new RequestLoginViewModel
        {
            PhoneNumber = phone ?? string.Empty,
            TenantId = resolvedTenantId
        };

        ViewData["Title"] = "ورود با پیامک";
        return View(model);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(RequestLoginViewModel model, CancellationToken cancellationToken)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "ورود با پیامک";
            return View(model);
        }

        model.TenantId = ResolveTenantId(model.TenantId);
        var command = new RequestCustomerLoginCommand(model.PhoneNumber, model.TenantId);
        try
        {
            var result = await _requestLoginHandler.HandleAsync(command, cancellationToken);
            TempData["LoginPhone"] = result.PhoneNumber;
            TempData["CodeExpiresAt"] = result.ExpiresAtUtc.ToString("O");
            TempData["TenantId"] = model.TenantId.ToString("D");
            TempData.Keep();
            return RedirectToAction(nameof(Verify), new { phone = result.PhoneNumber, tenantId = model.TenantId });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send login code for {Phone}", model.PhoneNumber);
            ModelState.AddModelError(string.Empty, "در ارسال کد ورود مشکلی رخ داد. لطفاً بعداً تلاش کنید.");
            ViewData["Title"] = "ورود با پیامک";
            return View(model);
        }
    }

    [HttpGet("verify")]
    [AllowAnonymous]
    public IActionResult Verify(string? phone = null, string? returnUrl = null, Guid? tenantId = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return Redirect(returnUrl ?? Url.Action("Index", "Home")!);
        }

        var phoneNumber = phone ?? TempData.Peek("LoginPhone") as string;
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return RedirectToAction(nameof(Login));
        }

        if (!tenantId.HasValue && TempData.Peek("TenantId") is string tenantString && Guid.TryParse(tenantString, out var parsedTenant))
        {
            tenantId = parsedTenant;
        }

        var model = new VerifyLoginViewModel
        {
            PhoneNumber = phoneNumber,
            ReturnUrl = returnUrl
        };

        if (TempData.TryGetValue("CodeExpiresAt", out var expiresAtObj) && expiresAtObj is string expiresAtString && DateTime.TryParse(expiresAtString, null, System.Globalization.DateTimeStyles.RoundtripKind, out var expiresAt))
        {
            ViewData["CodeExpiresAt"] = expiresAt;
            TempData.Keep("CodeExpiresAt");
        }

        ViewData["Title"] = "تأیید کد";
        return View(model);
    }

    [HttpPost("verify")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Verify(VerifyLoginViewModel model, CancellationToken cancellationToken)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return Redirect(model.ReturnUrl ?? Url.Action("Index", "Home")!);
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "تأیید کد";
            return View(model);
        }

        var command = new VerifyCustomerLoginCommand(model.PhoneNumber, model.Code);
        var result = await _verifyLoginHandler.HandleAsync(command, cancellationToken);
        if (!result.IsAuthenticated)
        {
            ModelState.AddModelError(string.Empty, result.FailureReason ?? "کد وارد شده معتبر نیست.");
            ViewData["Title"] = "تأیید کد";
            return View(model);
        }

        await SignInCustomerAsync(result.PhoneNumber, model.RememberMe);

        TempData.Remove("LoginPhone");
        TempData.Remove("CodeExpiresAt");
        TempData.Remove("TenantId");

        return Redirect(model.ReturnUrl ?? Url.Action("Index", "Home")!);
    }

    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        return Redirect(returnUrl ?? Url.Action("Index", "Home")!);
    }

    private Task SignInCustomerAsync(string phoneNumber, bool rememberMe)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, phoneNumber),
            new Claim(ClaimTypes.MobilePhone, phoneNumber)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = rememberMe,
            AllowRefresh = true
        };

        return HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);
    }

    private Guid ResolveTenantId(Guid? tenantId)
    {
        if (tenantId.HasValue && tenantId.Value != Guid.Empty)
        {
            return tenantId.Value;
        }

        return _tenantOptions.DefaultTenantId != Guid.Empty
            ? _tenantOptions.DefaultTenantId
            : Guid.Parse("11111111-1111-1111-1111-111111111111");
    }
}
