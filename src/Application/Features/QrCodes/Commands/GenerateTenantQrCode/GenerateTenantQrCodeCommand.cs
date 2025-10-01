namespace EazyMenu.Application.Features.QrCodes.Commands.GenerateTenantQrCode;

/// <summary>
/// تولید QR Code برای رستوران
/// </summary>
public sealed record GenerateTenantQrCodeCommand
{
    public Guid TenantId { get; init; }
    public string? CampaignName { get; init; }
}
