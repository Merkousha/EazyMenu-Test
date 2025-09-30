using System;
using System.Collections.Generic;
using System.Linq;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Menus.Common;

internal static class MenuEnumParser
{
    public static IReadOnlyDictionary<MenuChannel, Money> ToChannelPrices(IDictionary<string, decimal>? payload, string defaultCurrency)
    {
        if (payload is null || payload.Count == 0)
        {
            return new Dictionary<MenuChannel, Money>();
        }

        return payload.ToDictionary(
            pair => ParseChannel(pair.Key),
            pair => Money.From(pair.Value, defaultCurrency));
    }

    public static IReadOnlyCollection<MenuTag> ToTags(IReadOnlyCollection<string>? tags)
    {
        if (tags is null || tags.Count == 0)
        {
            return Array.Empty<MenuTag>();
        }

        return tags.Select(ParseTag).Distinct().ToList();
    }

    private static MenuChannel ParseChannel(string value)
    {
        if (Enum.TryParse<MenuChannel>(value, true, out var channel))
        {
            return channel;
        }

        throw new BusinessRuleViolationException($"کانال قیمت '{value}' معتبر نیست.");
    }

    private static MenuTag ParseTag(string value)
    {
        if (Enum.TryParse<MenuTag>(value, true, out var tag))
        {
            return tag;
        }

        throw new BusinessRuleViolationException($"برچسب '{value}' معتبر نیست.");
    }
}
