using EazyMenu.Application.Abstractions.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Features.Menus.Common;
using EazyMenu.Application.Features.Menus.Queries.GetPublishedMenu;
using EazyMenu.Public.Controllers;
using EazyMenu.Public.Models.Menus;
using EazyMenu.Public.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace EazyMenu.UnitTests.Presentation.Public.Controllers;

public sealed class MenusControllerTests
{
    private readonly Mock<IQueryHandler<GetPublishedMenuQuery, PublishedMenuDto?>> _queryHandler = new();
    private readonly TenantSiteOptions _tenantOptions = new() { DefaultTenantId = Guid.NewGuid() };

    [Fact]
    public async Task IndexByTenant_WhenSnapshotExists_ReturnsViewWithMenu()
    {
        var snapshot = new PublishedMenuDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Version: 2,
            Name: new()
            {
                ["fa-IR"] = "منوی تست"
            },
            Description: null,
            PublishedAtUtc: DateTime.UtcNow,
            Categories: Array.Empty<PublishedMenuCategoryDto>());

        _queryHandler
            .Setup(handler => handler.HandleAsync(It.IsAny<GetPublishedMenuQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(snapshot);

        var controller = CreateController();

        var result = await controller.IndexByTenant(_tenantOptions.DefaultTenantId, null, null, CancellationToken.None);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", view.ViewName);
        var model = Assert.IsType<PublicMenuPageViewModel>(view.Model);
        Assert.True(model.HasMenu);
        Assert.NotNull(model.Menu);
        Assert.Equal("منوی تست", model.Menu!.DisplayName);
        Assert.Null(model.SearchTerm);
        Assert.False(model.HasResults);
    }

    [Fact]
    public async Task IndexByTenant_WhenSnapshotMissing_ReturnsEmptyState()
    {
        _queryHandler
            .Setup(handler => handler.HandleAsync(It.IsAny<GetPublishedMenuQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PublishedMenuDto?)null);

        var controller = CreateController();

        var result = await controller.IndexByTenant(_tenantOptions.DefaultTenantId, null, null, CancellationToken.None);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", view.ViewName);
        var model = Assert.IsType<PublicMenuPageViewModel>(view.Model);
        Assert.False(model.HasMenu);
        Assert.Null(model.Menu);
        Assert.Null(model.SearchTerm);
        Assert.False(model.HasResults);
    }

    [Fact]
    public async Task IndexByTenant_WithSearchQuery_FiltersMenuAndPersistsQuery()
    {
        var tenantId = _tenantOptions.DefaultTenantId;
        var matchingCategoryId = Guid.NewGuid();
        var snapshot = new PublishedMenuDto(
            tenantId,
            Guid.NewGuid(),
            Version: 3,
            Name: new()
            {
                ["fa-IR"] = "منوی پاییزی"
            },
            Description: null,
            PublishedAtUtc: DateTime.UtcNow,
            Categories: new[]
            {
                new PublishedMenuCategoryDto(
                    matchingCategoryId,
                    new()
                    {
                        ["fa-IR"] = "پیتزا"
                    },
                    IconUrl: null,
                    DisplayOrder: 1,
                    Items: new[]
                    {
                        new PublishedMenuItemDto(
                            Guid.NewGuid(),
                            new() { ["fa-IR"] = "پیتزا مخصوص" },
                            new() { ["fa-IR"] = "سرشار از پنیر" },
                            BasePrice: 290000,
                            Currency: "IRR",
                            IsAvailable: true,
                            Inventory: new InventoryDetailsDto("in-stock", 10, 2, false),
                            ImageUrl: null,
                            DisplayOrder: 1,
                ChannelPrices: new Dictionary<string, decimal>(),
                Tags: new[] { "خانوادگی" })
            }),
                new PublishedMenuCategoryDto(
                    Guid.NewGuid(),
                    new()
                    {
                        ["fa-IR"] = "نوشیدنی"
                    },
                    IconUrl: null,
                    DisplayOrder: 2,
                    Items: new[]
                    {
                        new PublishedMenuItemDto(
                            Guid.NewGuid(),
                            new() { ["fa-IR"] = "آب معدنی" },
                            null,
                            BasePrice: 15000,
                            Currency: "IRR",
                            IsAvailable: true,
                            Inventory: new InventoryDetailsDto("in-stock", 50, 10, false),
                            ImageUrl: null,
                            DisplayOrder: 1,
                            ChannelPrices: new Dictionary<string, decimal>(),
                            Tags: Array.Empty<string>())
                    })
            });

        _queryHandler
            .Setup(handler => handler.HandleAsync(It.IsAny<GetPublishedMenuQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(snapshot);

        var controller = CreateController();

        var result = await controller.IndexByTenant(tenantId, null, "  پیتزا  ", CancellationToken.None);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<PublicMenuPageViewModel>(view.Model);
        Assert.Equal("پیتزا", model.SearchTerm);
        Assert.True(model.HasResults);
        Assert.NotNull(model.Menu);
        var categories = model.Menu!.Categories;
        Assert.Single(categories);
        Assert.Equal(matchingCategoryId, categories.First().CategoryId);
        Assert.Single(categories.First().Items);
        Assert.Equal("پیتزا مخصوص", categories.First().Items.First().DisplayName);
    }

    private MenusController CreateController()
    {
        var tenantRepo = new Mock<ITenantRepository>().Object;
        return new MenusController(
            NullLogger<MenusController>.Instance,
            _queryHandler.Object,
            tenantRepo,
            Options.Create(_tenantOptions));
    }
}
