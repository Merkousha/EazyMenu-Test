using System;
using System.Collections.Generic;
using EazyMenu.Domain.Common;
using EazyMenu.Domain.Common.Guards;

namespace EazyMenu.Domain.ValueObjects;

/// <summary>
/// پروفایل برند و ظاهر سایت عمومی رستوران
/// شامل نام نمایشی، لوگو، رنگ‌بندی، تصاویر و اطلاعات نمایشی
/// </summary>
public sealed class BrandProfile : ValueObject
{
    public string DisplayName { get; }

    public string? LogoUrl { get; }

    public string? PrimaryColor { get; }

    public string? SecondaryColor { get; }

    public string? BannerImageUrl { get; }

    public string? AboutText { get; }

    public string? OpeningHours { get; }

    public string TemplateName { get; }

    public bool IsPublished { get; }

    private BrandProfile(
        string displayName,
        string? logoUrl,
        string? primaryColor,
        string? secondaryColor,
        string? bannerImageUrl,
        string? aboutText,
        string? openingHours,
        string templateName,
        bool isPublished)
    {
        Guard.AgainstNullOrWhiteSpace(displayName, nameof(displayName));
        Guard.AgainstNullOrWhiteSpace(templateName, nameof(templateName));

        DisplayName = displayName.Trim();
        LogoUrl = string.IsNullOrWhiteSpace(logoUrl) ? null : logoUrl.Trim();
        PrimaryColor = string.IsNullOrWhiteSpace(primaryColor) ? null : primaryColor.Trim().ToLowerInvariant();
        SecondaryColor = string.IsNullOrWhiteSpace(secondaryColor) ? null : secondaryColor.Trim().ToLowerInvariant();
        BannerImageUrl = string.IsNullOrWhiteSpace(bannerImageUrl) ? null : bannerImageUrl.Trim();
        AboutText = string.IsNullOrWhiteSpace(aboutText) ? null : aboutText.Trim();
        OpeningHours = string.IsNullOrWhiteSpace(openingHours) ? null : openingHours.Trim();
        TemplateName = templateName.Trim();
        IsPublished = isPublished;
    }

    public static BrandProfile Create(
        string displayName,
        string? logoUrl = null,
        string? primaryColor = null,
        string? secondaryColor = null,
        string templateName = "classic")
    {
        return new BrandProfile(
            displayName,
            logoUrl,
            primaryColor,
            secondaryColor,
            null,
            null,
            null,
            templateName,
            false);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return DisplayName.ToLowerInvariant();
        yield return LogoUrl?.ToLowerInvariant();
        yield return PrimaryColor;
        yield return SecondaryColor;
        yield return BannerImageUrl?.ToLowerInvariant();
        yield return AboutText;
        yield return OpeningHours;
        yield return TemplateName.ToLowerInvariant();
        yield return IsPublished;
    }

    public BrandProfile Update(
        string? displayName = null,
        string? logoUrl = null,
        string? primaryColor = null,
        string? secondaryColor = null,
        string? bannerImageUrl = null,
        string? aboutText = null,
        string? openingHours = null,
        string? templateName = null)
    {
        return new BrandProfile(
            displayName ?? DisplayName,
            logoUrl ?? LogoUrl,
            primaryColor ?? PrimaryColor,
            secondaryColor ?? SecondaryColor,
            bannerImageUrl ?? BannerImageUrl,
            aboutText ?? AboutText,
            openingHours ?? OpeningHours,
            templateName ?? TemplateName,
            IsPublished);
    }

    public BrandProfile Publish()
    {
        return new BrandProfile(
            DisplayName,
            LogoUrl,
            PrimaryColor,
            SecondaryColor,
            BannerImageUrl,
            AboutText,
            OpeningHours,
            TemplateName,
            true);
    }

    public BrandProfile Unpublish()
    {
        return new BrandProfile(
            DisplayName,
            LogoUrl,
            PrimaryColor,
            SecondaryColor,
            BannerImageUrl,
            AboutText,
            OpeningHours,
            TemplateName,
            false);
    }

    public BrandProfile ChangeTemplate(string templateName)
    {
        Guard.AgainstNullOrWhiteSpace(templateName, nameof(templateName));

        return new BrandProfile(
            DisplayName,
            LogoUrl,
            PrimaryColor,
            SecondaryColor,
            BannerImageUrl,
            AboutText,
            OpeningHours,
            templateName,
            IsPublished);
    }
}
