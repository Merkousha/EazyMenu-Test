using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Tenants.Queries.GetTenantBranding;

public sealed class GetTenantBrandingHandler
{
    private readonly ITenantRepository _tenantRepository;

    public GetTenantBrandingHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<BrandingDto?> HandleAsync(GetTenantBrandingQuery query, CancellationToken cancellationToken = default)
    {
        var tenantId = TenantId.FromGuid(query.TenantId);
        var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
        
        if (tenant == null)
            return null;

        return new BrandingDto
        {
            DisplayName = tenant.Branding.DisplayName,
            LogoUrl = tenant.Branding.LogoUrl,
            PrimaryColor = tenant.Branding.PrimaryColor,
            SecondaryColor = tenant.Branding.SecondaryColor,
            BannerImageUrl = tenant.Branding.BannerImageUrl,
            AboutText = tenant.Branding.AboutText,
            OpeningHours = tenant.Branding.OpeningHours,
            TemplateName = tenant.Branding.TemplateName,
            IsPublished = tenant.Branding.IsPublished
        };
    }
}

public sealed record BrandingDto
{
    public string DisplayName { get; init; } = string.Empty;
    public string? LogoUrl { get; init; }
    public string? PrimaryColor { get; init; }
    public string? SecondaryColor { get; init; }
    public string? BannerImageUrl { get; init; }
    public string? AboutText { get; init; }
    public string? OpeningHours { get; init; }
    public string TemplateName { get; init; } = string.Empty;
    public bool IsPublished { get; init; }
}
