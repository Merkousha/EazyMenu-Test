namespace EazyMenu.Application.Features.Tenants.Commands.UpdateBranding;

/// <summary>
/// به‌روزرسانی برندینگ و ظاهر سایت عمومی رستوران
/// </summary>
public sealed record UpdateBrandingCommand
{
    public Guid TenantId { get; init; }
    public string? DisplayName { get; init; }
    public string? LogoUrl { get; init; }
    public string? PrimaryColor { get; init; }
    public string? SecondaryColor { get; init; }
    public string? BannerImageUrl { get; init; }
    public string? AboutText { get; init; }
    public string? OpeningHours { get; init; }
    public string? TemplateName { get; init; }
    public bool? ShouldPublish { get; init; }
}
