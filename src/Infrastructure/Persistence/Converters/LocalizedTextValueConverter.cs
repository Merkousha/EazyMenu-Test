using System;
using System.Collections.Generic;
using System.Text.Json;
using EazyMenu.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EazyMenu.Infrastructure.Persistence.Converters;

internal sealed class LocalizedTextValueConverter : ValueConverter<LocalizedText, string>
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.General);

    public LocalizedTextValueConverter()
        : base(
            text => JsonSerializer.Serialize(text.Values, SerializerOptions),
            json => LocalizedText.FromDictionary(
                JsonSerializer.Deserialize<Dictionary<string, string>>(json, SerializerOptions)
                ?? new Dictionary<string, string> { { LocalizedText.DefaultCulture, string.Empty } }))
    {
    }
}
