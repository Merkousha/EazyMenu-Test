using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using EazyMenu.Domain.Common;
using EazyMenu.Domain.Common.Exceptions;
using EazyMenu.Domain.Common.Guards;

namespace EazyMenu.Domain.ValueObjects;

/// <summary>
/// شناسه یکتای URL-friendly برای رستوران (مثلاً: my-restaurant)
/// </summary>
public sealed class TenantSlug : ValueObject
{
    public string Value { get; }

    private TenantSlug(string value)
    {
        Guard.AgainstNullOrWhiteSpace(value, nameof(value));

        var normalized = value.Trim().ToLowerInvariant();

        // Validate slug format: lowercase alphanumeric with hyphens
        if (!Regex.IsMatch(normalized, @"^[a-z0-9-]+$"))
        {
            throw new DomainException("Slug باید فقط شامل حروف انگلیسی کوچک، اعداد و خط تیره باشد");
        }

        if (normalized.Length < 3)
        {
            throw new DomainException("Slug باید حداقل 3 کاراکتر باشد");
        }

        if (normalized.Length > 50)
        {
            throw new DomainException("Slug نباید بیشتر از 50 کاراکتر باشد");
        }

        Value = normalized;
    }

    public static TenantSlug Create(string value) => new(value);

    /// <summary>
    /// تولید Slug از روی نام کسب‌وکار
    /// </summary>
    public static TenantSlug FromBusinessName(string businessName)
    {
        Guard.AgainstNullOrWhiteSpace(businessName, nameof(businessName));

        // Simple slugify: convert to lowercase, replace spaces with hyphens
        var slug = businessName.Trim()
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("_", "-");

        // Remove non-alphanumeric characters (except hyphens)
        slug = Regex.Replace(slug, @"[^a-z0-9-]", "");

        // Remove consecutive hyphens
        slug = Regex.Replace(slug, @"-+", "-");

        // Trim hyphens from start and end
        slug = slug.Trim('-');

        // If empty or too short, generate random
        if (string.IsNullOrWhiteSpace(slug) || slug.Length < 3)
        {
            slug = $"tenant-{Guid.NewGuid().ToString("N")[..8]}";
        }

        // Limit length
        if (slug.Length > 50)
        {
            slug = slug.Substring(0, 50).TrimEnd('-');
        }

        return new TenantSlug(slug);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(TenantSlug slug) => slug.Value;
}
