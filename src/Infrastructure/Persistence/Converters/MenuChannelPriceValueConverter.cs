using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using EazyMenu.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EazyMenu.Infrastructure.Persistence.Converters;

internal sealed class MenuChannelPriceValueConverter : ValueConverter<Dictionary<MenuChannel, Money>, string>
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.General);

    public MenuChannelPriceValueConverter()
        : base(prices => Serialize(prices), json => Deserialize(json))
    {
    }

    private static string Serialize(Dictionary<MenuChannel, Money> prices)
    {
        var payload = prices.Select(pair => new ChannelPriceDto(pair.Key.ToString(), pair.Value.Amount, pair.Value.Currency));
        return JsonSerializer.Serialize(payload, SerializerOptions);
    }

    private static Dictionary<MenuChannel, Money> Deserialize(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new Dictionary<MenuChannel, Money>();
        }

        var payload = JsonSerializer.Deserialize<List<ChannelPriceDto>>(json, SerializerOptions) ?? new List<ChannelPriceDto>();
        return payload.ToDictionary(
            dto => Enum.Parse<MenuChannel>(dto.Channel, true),
            dto => Money.From(dto.Amount, dto.Currency));
    }

    private sealed record ChannelPriceDto(string Channel, decimal Amount, string Currency);
}
