using System;
using System.Collections.Generic;
using EazyMenu.Application.Features.Menus.Common;
using EazyMenu.Public.Models.Menus;
using Xunit;

namespace EazyMenu.UnitTests.Presentation.Public.Models;

public sealed class PublicMenuViewModelFactoryTests
{
    [Fact]
    public void CreatePageModel_WithSearchQueryAndNoMatches_ReturnsEmptyCategories()
    {
        var tenantId = Guid.NewGuid();
        var snapshot = new PublishedMenuDto(
            tenantId,
            Guid.NewGuid(),
            Version: 1,
            Name: new()
            {
                ["fa-IR"] = "منوی اصلی"
            },
            Description: null,
            PublishedAtUtc: DateTime.UtcNow,
            Categories: new[]
            {
                new PublishedMenuCategoryDto(
                    Guid.NewGuid(),
                    new() { ["fa-IR"] = "پیش‌غذا" },
                    IconUrl: null,
                    DisplayOrder: 1,
                    Items: new[]
                    {
                        new PublishedMenuItemDto(
                            Guid.NewGuid(),
                            new() { ["fa-IR"] = "سالاد سزار" },
                            new() { ["fa-IR"] = "سس مخصوص" },
                            BasePrice: 180000,
                            Currency: "IRR",
                            IsAvailable: true,
                            Inventory: new InventoryDetailsDto("in-stock", 5, 1, false),
                            ImageUrl: null,
                            DisplayOrder: 1,
                            ChannelPrices: new Dictionary<string, decimal>(),
                            Tags: new[] { "سبک" })
                    })
            });

        var pageModel = PublicMenuViewModelFactory.CreatePageModel(snapshot, tenantId, searchTerm: "دسر");

        Assert.True(pageModel.HasMenu);
        Assert.False(pageModel.HasResults);
        Assert.Equal("دسر", pageModel.SearchTerm);
        Assert.NotNull(pageModel.Menu);
        Assert.Empty(pageModel.Menu!.Categories);
    }
}
