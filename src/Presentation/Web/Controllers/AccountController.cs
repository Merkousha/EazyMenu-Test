using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Features.Identity.ChangePassword;
using EazyMenu.Application.Features.Identity.Login;
using EazyMenu.Application.Features.Identity.RegisterUser;
using EazyMenu.Domain.ValueObjects;
using EazyMenu.Web.Models.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EazyMenu.Web.Controllers;

public class AccountController : Controller
{
    private readonly LoginCommandHandler _loginHandler;
    private readonly RegisterUserCommandHandler _registerHandler;
    private readonly ChangePasswordCommandHandler _changePasswordHandler;

    public AccountController(
        LoginCommandHandler loginHandler,
        RegisterUserCommandHandler registerHandler,
        ChangePasswordCommandHandler changePasswordHandler)
    {
        _loginHandler = loginHandler;
        _registerHandler = registerHandler;
        _changePasswordHandler = changePasswordHandler;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var command = new LoginCommand(model.Email, model.Password);
        var result = await _loginHandler.HandleAsync(command, cancellationToken);

        if (!result.IsSuccessful || result.User is null)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "ورود ناموفق بود.");
            return View(model);
        }

        // Create claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, result.User.UserId.ToString()),
            new Claim(ClaimTypes.Email, result.User.Email),
            new Claim(ClaimTypes.Name, result.User.FullName),
            new Claim(ClaimTypes.Role, result.User.Role.ToString()),
            new Claim("TenantId", result.User.TenantId.ToString())
        };

        if (!string.IsNullOrEmpty(result.User.PhoneNumber))
        {
            claims.Add(new Claim(ClaimTypes.MobilePhone, result.User.PhoneNumber));
        }

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            ExpiresUtc = model.RememberMe 
                ? DateTimeOffset.UtcNow.AddDays(30) 
                : DateTimeOffset.UtcNow.AddHours(12)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Dashboard");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // For now, we assume TenantId comes from somewhere (e.g., subdomain or registration flow)
        // This is a simplified version - in production, you'd get TenantId from tenant registration
        var tenantId = TenantId.New(); // TODO: Get from tenant creation flow

        var command = new RegisterUserCommand(
            tenantId.Value,
            model.Email,
            model.FullName,
            model.PhoneNumber ?? string.Empty,
            model.Password,
            Domain.Aggregates.Users.UserRole.Owner // First user is always Owner
        );

        try
        {
            var userId = await _registerHandler.HandleAsync(command, cancellationToken);
            TempData["SuccessMessage"] = "ثبت نام با موفقیت انجام شد. لطفا وارد شوید.";
            return RedirectToAction(nameof(Login));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }

    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [Authorize]
    public IActionResult ChangePassword()
    {
        return View();
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userIdGuid))
        {
            return Unauthorized();
        }

        var command = new ChangePasswordCommand(
            userIdGuid,
            model.CurrentPassword,
            model.NewPassword
        );

        var result = await _changePasswordHandler.HandleAsync(command, cancellationToken);

        if (!result)
        {
            ModelState.AddModelError(string.Empty, "تغییر رمز عبور ناموفق بود. لطفا رمز عبور فعلی را بررسی کنید.");
            return View(model);
        }

        TempData["SuccessMessage"] = "رمز عبور با موفقیت تغییر کرد.";
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
