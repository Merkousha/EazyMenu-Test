using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using EazyMenu.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EazyMenu.Infrastructure.Persistence.Converters;

internal sealed class MenuTagCollectionValueConverter : ValueConverter<HashSet<MenuTag>, string>
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.General);

    public MenuTagCollectionValueConverter()
        : base(tags => Serialize(tags), json => Deserialize(json))
    {
    }

    private static string Serialize(HashSet<MenuTag> tags)
    {
        var payload = tags.Select(tag => tag.ToString());
        return JsonSerializer.Serialize(payload, SerializerOptions);
    }

    private static HashSet<MenuTag> Deserialize(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new HashSet<MenuTag>();
        }

        var payload = JsonSerializer.Deserialize<List<string>>(json, SerializerOptions) ?? new List<string>();
        return new HashSet<MenuTag>(payload.Select(value => Enum.Parse<MenuTag>(value, true)));
    }
}
