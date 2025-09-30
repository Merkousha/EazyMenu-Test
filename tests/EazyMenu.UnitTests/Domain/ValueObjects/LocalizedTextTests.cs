using System.Collections.Generic;
using EazyMenu.Domain.ValueObjects;
using Xunit;

namespace EazyMenu.UnitTests.Domain.ValueObjects;

public class LocalizedTextTests
{
    [Fact]
    public void Create_ShouldStoreDefaultCulture()
    {
        var text = LocalizedText.Create("ساندویچ ویژه");

        Assert.Equal("ساندویچ ویژه", text.GetValue(LocalizedText.DefaultCulture));
    }

    [Fact]
    public void FromDictionary_ShouldNormalizeCultures()
    {
        var values = new Dictionary<string, string>
        {
            { "fa-ir", "پیتزا" },
            { "EN-us", "Pizza" }
        };

        var text = LocalizedText.FromDictionary(values);

        Assert.Equal("پیتزا", text.GetValue("fa-IR"));
        Assert.Equal("Pizza", text.GetValue("en-US"));
    }

    [Fact]
    public void WithValue_ShouldOverrideExistingCulture()
    {
        var text = LocalizedText.Create("قهوه ترک");
        var updated = text.WithValue("en-US", "Turkish Coffee");

        Assert.Equal("Turkish Coffee", updated.GetValue("en-US"));
        Assert.Equal("قهوه ترک", updated.GetValue("fa-IR"));
    }
}
