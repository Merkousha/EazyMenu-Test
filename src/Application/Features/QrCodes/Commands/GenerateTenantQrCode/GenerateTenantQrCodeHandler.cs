using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Domain.Aggregates.Tenants;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.QrCodes.Commands.GenerateTenantQrCode;

public sealed class GenerateTenantQrCodeHandler
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IQrCodeGenerator _qrCodeGenerator;

    public GenerateTenantQrCodeHandler(
        ITenantRepository tenantRepository,
        IQrCodeGenerator qrCodeGenerator)
    {
        _tenantRepository = tenantRepository;
        _qrCodeGenerator = qrCodeGenerator;
    }

    public async Task<QrCodeResult> HandleAsync(GenerateTenantQrCodeCommand command, CancellationToken cancellationToken = default)
    {
        var tenantId = TenantId.FromGuid(command.TenantId);
        var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);

        if (tenant == null)
            throw new InvalidOperationException($"رستوران با شناسه {command.TenantId} یافت نشد");

        // ساخت URL برای منوی عمومی - فعلاً از slug استفاده می‌کنیم
        var publicMenuUrl = $"/menu/{tenant.Slug.Value}";

        // اگر کمپین داریم، به URL اضافه می‌کنیم
        if (!string.IsNullOrWhiteSpace(command.CampaignName))
        {
            publicMenuUrl += $"?campaign={Uri.EscapeDataString(command.CampaignName)}";
        }

        // تولید QR Code
        var qrCodePng = _qrCodeGenerator.GenerateQrCodePng(publicMenuUrl, 10);
        var qrCodeBase64 = _qrCodeGenerator.GenerateQrCodeBase64(publicMenuUrl, 10);
        var qrCodeSvg = _qrCodeGenerator.GenerateQrCodeSvg(publicMenuUrl, 10);

        return new QrCodeResult
        {
            TenantId = command.TenantId,
            TenantName = tenant.BusinessName,
            PublicMenuUrl = publicMenuUrl,
            QrCodePng = qrCodePng,
            QrCodeBase64 = qrCodeBase64,
            QrCodeSvg = qrCodeSvg,
            CampaignName = command.CampaignName
        };
    }
}

public sealed record QrCodeResult
{
    public Guid TenantId { get; init; }
    public string TenantName { get; init; } = string.Empty;
    public string PublicMenuUrl { get; init; } = string.Empty;
    public byte[] QrCodePng { get; init; } = Array.Empty<byte>();
    public string QrCodeBase64 { get; init; } = string.Empty;
    public string QrCodeSvg { get; init; } = string.Empty;
    public string? CampaignName { get; init; }
}
