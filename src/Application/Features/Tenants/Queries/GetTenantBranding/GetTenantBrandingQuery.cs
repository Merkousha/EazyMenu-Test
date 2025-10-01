namespace EazyMenu.Application.Features.Tenants.Queries.GetTenantBranding;

public sealed record GetTenantBrandingQuery
{
    public Guid TenantId { get; init; }
}
