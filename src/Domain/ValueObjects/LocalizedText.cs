using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using EazyMenu.Domain.Common;
using EazyMenu.Domain.Common.Guards;

namespace EazyMenu.Domain.ValueObjects;

public sealed class LocalizedText : ValueObject
{
    public const string DefaultCulture = "fa-IR";
    public static readonly IReadOnlyCollection<string> SupportedCultures = new ReadOnlyCollection<string>(new[] { "fa-IR", "en-US", "ar-SA" });

    private readonly Dictionary<string, string> _values;

    private LocalizedText(Dictionary<string, string> values)
    {
        if (values.Count == 0)
        {
            throw new ArgumentException("حداقل یک مقدار محلی لازم است.", nameof(values));
        }

        _values = values;
    }

    public IReadOnlyDictionary<string, string> Values => new ReadOnlyDictionary<string, string>(_values);

    public string GetValue(string culture)
    {
        var normalizedCulture = NormalizeCulture(culture);
        if (_values.TryGetValue(normalizedCulture, out var value))
        {
            return value;
        }

        throw new KeyNotFoundException($"مقدار زبان '{normalizedCulture}' یافت نشد.");
    }

    public string GetValueOrDefault(string culture, string fallbackCulture = DefaultCulture)
    {
        var normalizedCulture = NormalizeCulture(culture);
        if (_values.TryGetValue(normalizedCulture, out var value))
        {
            return value;
        }

        var normalizedFallback = NormalizeCulture(fallbackCulture);
        if (_values.TryGetValue(normalizedFallback, out var fallback))
        {
            return fallback;
        }

        return _values.Values.First();
    }

    public LocalizedText WithValue(string culture, string value)
    {
        var normalizedCulture = NormalizeCulture(culture);
        var sanitizedValue = SanitizeValue(value, normalizedCulture);
        var clone = new Dictionary<string, string>(_values, StringComparer.OrdinalIgnoreCase)
        {
            [normalizedCulture] = sanitizedValue
        };

        return new LocalizedText(NormalizeDictionary(clone));
    }

    public static LocalizedText Create(string value, string culture = DefaultCulture)
    {
        var normalizedCulture = NormalizeCulture(culture);
        var sanitizedValue = SanitizeValue(value, normalizedCulture);
        var dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [normalizedCulture] = sanitizedValue
        };

        return new LocalizedText(NormalizeDictionary(dictionary));
    }

    public static LocalizedText FromDictionary(IDictionary<string, string> values)
    {
        Guard.AgainstNull(values, nameof(values));

        var normalized = values
            .Where(pair => !string.IsNullOrWhiteSpace(pair.Value))
            .ToDictionary(pair => NormalizeCulture(pair.Key), pair => SanitizeValue(pair.Value, pair.Key), StringComparer.OrdinalIgnoreCase);

        if (normalized.Count == 0)
        {
            throw new ArgumentException("حداقل یک مقدار محلی لازم است.", nameof(values));
        }

        return new LocalizedText(NormalizeDictionary(normalized));
    }

    public override string ToString()
    {
        var defaultValue = GetValueOrDefault(DefaultCulture, DefaultCulture);
        return defaultValue;
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        foreach (var kvp in _values.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
        {
            yield return kvp.Key;
            yield return kvp.Value;
        }
    }

    private static string NormalizeCulture(string culture)
    {
        Guard.AgainstNullOrWhiteSpace(culture, nameof(culture));
        var cultureInfo = CultureInfo.GetCultureInfo(culture);
        var normalized = cultureInfo.Name;

        if (!SupportedCultures.Contains(normalized))
        {
            throw new ArgumentException($"فرهنگ '{normalized}' پشتیبانی نمی‌شود.", nameof(culture));
        }

        return normalized;
    }

    private static Dictionary<string, string> NormalizeDictionary(IDictionary<string, string> source)
    {
        var normalized = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var pair in source)
        {
            var culture = NormalizeCulture(pair.Key);
            normalized[culture] = pair.Value.Trim();
        }

        return normalized;
    }

    private static string SanitizeValue(string value, string culture)
    {
        Guard.AgainstNullOrWhiteSpace(value, nameof(value));
        var trimmed = value.Trim();

        if (trimmed.Length > 1024)
        {
            throw new ArgumentException($"متن زبان '{culture}' نباید بیش از 1024 کاراکتر باشد.", nameof(value));
        }

        return trimmed;
    }
}
