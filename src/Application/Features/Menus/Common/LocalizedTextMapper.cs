using System.Collections.Generic;
using System.Linq;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Menus.Common;

internal static class LocalizedTextMapper
{
    public static LocalizedText ToLocalizedText(IDictionary<string, string> values)
    {
        var sanitized = values
            .Where(pair => !string.IsNullOrWhiteSpace(pair.Value))
            .ToDictionary(pair => pair.Key, pair => pair.Value.Trim());

        if (sanitized.Count == 0)
        {
            throw new System.ArgumentException("حداقل یک مقدار متنی معتبر لازم است.", nameof(values));
        }

        return LocalizedText.FromDictionary(sanitized);
    }

    public static Dictionary<string, string> ToDictionary(LocalizedText text)
    {
        return text.Values.ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    public static Dictionary<string, string>? ToDictionaryOrNull(LocalizedText? text)
    {
        return text is null ? null : ToDictionary(text);
    }
}
