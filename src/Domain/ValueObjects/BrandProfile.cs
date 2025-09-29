using System;
using System.Collections.Generic;
using EazyMenu.Domain.Common;
using EazyMenu.Domain.Common.Guards;

namespace EazyMenu.Domain.ValueObjects;

public sealed class BrandProfile : ValueObject
{
    public string DisplayName { get; }

    public string? LogoUrl { get; }

    public string? PrimaryColor { get; }

    private BrandProfile(string displayName, string? logoUrl, string? primaryColor)
    {
        Guard.AgainstNullOrWhiteSpace(displayName, nameof(displayName));

        DisplayName = displayName.Trim();
        LogoUrl = string.IsNullOrWhiteSpace(logoUrl) ? null : logoUrl.Trim();
        PrimaryColor = string.IsNullOrWhiteSpace(primaryColor) ? null : primaryColor.Trim().ToLowerInvariant();
    }

    public static BrandProfile Create(string displayName, string? logoUrl = null, string? primaryColor = null)
    {
        return new BrandProfile(displayName, logoUrl, primaryColor);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return DisplayName.ToLowerInvariant();
        yield return LogoUrl?.ToLowerInvariant();
        yield return PrimaryColor;
    }

    public BrandProfile Update(string? displayName = null, string? logoUrl = null, string? primaryColor = null)
    {
        return new BrandProfile(displayName ?? DisplayName, logoUrl ?? LogoUrl, primaryColor ?? PrimaryColor);
    }
}
