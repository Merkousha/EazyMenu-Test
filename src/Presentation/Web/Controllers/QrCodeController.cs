using EazyMenu.Application.Features.QrCodes.Commands.GenerateTenantQrCode;
using EazyMenu.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EazyMenu.Web.Controllers;

[Authorize]
[Route("qr-code")]
public sealed class QrCodeController : Controller
{
    private readonly GenerateTenantQrCodeHandler _generateQrCodeHandler;
    private readonly ILogger<QrCodeController> _logger;

    public QrCodeController(
        GenerateTenantQrCodeHandler generateQrCodeHandler,
        ILogger<QrCodeController> logger)
    {
        _generateQrCodeHandler = generateQrCodeHandler;
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

        try
        {
            var command = new GenerateTenantQrCodeCommand { TenantId = tenantId.Value };
            var result = await _generateQrCodeHandler.HandleAsync(command, cancellationToken);

            ViewData["Title"] = "QR Code رستوران";
            return View(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating QR code for tenant {TenantId}", tenantId.Value);
            TempData["ErrorMessage"] = "خطا در تولید QR Code";
            return RedirectToAction("Index", "Dashboard");
        }
    }

    [HttpGet("download/png")]
    public async Task<IActionResult> DownloadPng(string? campaign, CancellationToken cancellationToken)
    {
        var tenantId = User.GetTenantId();
        if (!tenantId.HasValue)
        {
            return NotFound();
        }

        try
        {
            var command = new GenerateTenantQrCodeCommand 
            { 
                TenantId = tenantId.Value,
                CampaignName = campaign
            };
            
            var result = await _generateQrCodeHandler.HandleAsync(command, cancellationToken);

            var fileName = string.IsNullOrWhiteSpace(campaign)
                ? "menu-qrcode.png"
                : $"menu-qrcode-{campaign}.png";

            return File(result.QrCodePng, "image/png", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading QR code PNG for tenant {TenantId}", tenantId.Value);
            return StatusCode(500);
        }
    }

    [HttpGet("download/svg")]
    public async Task<IActionResult> DownloadSvg(string? campaign, CancellationToken cancellationToken)
    {
        var tenantId = User.GetTenantId();
        if (!tenantId.HasValue)
        {
            return NotFound();
        }

        try
        {
            var command = new GenerateTenantQrCodeCommand 
            { 
                TenantId = tenantId.Value,
                CampaignName = campaign
            };
            
            var result = await _generateQrCodeHandler.HandleAsync(command, cancellationToken);

            var fileName = string.IsNullOrWhiteSpace(campaign)
                ? "menu-qrcode.svg"
                : $"menu-qrcode-{campaign}.svg";

            return File(System.Text.Encoding.UTF8.GetBytes(result.QrCodeSvg), "image/svg+xml", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading QR code SVG for tenant {TenantId}", tenantId.Value);
            return StatusCode(500);
        }
    }

    [HttpPost("generate-campaign")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateCampaignQrCode(string campaignName, CancellationToken cancellationToken)
    {
        var tenantId = User.GetTenantId();
        if (!tenantId.HasValue)
        {
            return Json(new { success = false, error = "شناسه رستوران یافت نشد" });
        }

        if (string.IsNullOrWhiteSpace(campaignName))
        {
            return Json(new { success = false, error = "نام کمپین الزامی است" });
        }

        try
        {
            var command = new GenerateTenantQrCodeCommand 
            { 
                TenantId = tenantId.Value,
                CampaignName = campaignName
            };
            
            var result = await _generateQrCodeHandler.HandleAsync(command, cancellationToken);

            return Json(new 
            { 
                success = true, 
                qrCodeBase64 = result.QrCodeBase64,
                url = result.PublicMenuUrl,
                campaignName = result.CampaignName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating campaign QR code for tenant {TenantId}", tenantId.Value);
            return Json(new { success = false, error = "خطا در تولید QR Code" });
        }
    }
}
