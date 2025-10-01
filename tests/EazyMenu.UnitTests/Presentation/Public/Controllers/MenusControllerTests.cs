using System;
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

        var result = await controller.IndexByTenant(_tenantOptions.DefaultTenantId, null, CancellationToken.None);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", view.ViewName);
        var model = Assert.IsType<PublicMenuPageViewModel>(view.Model);
        Assert.True(model.HasMenu);
        Assert.NotNull(model.Menu);
        Assert.Equal("منوی تست", model.Menu!.DisplayName);
    }

    [Fact]
    public async Task IndexByTenant_WhenSnapshotMissing_ReturnsEmptyState()
    {
        _queryHandler
            .Setup(handler => handler.HandleAsync(It.IsAny<GetPublishedMenuQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PublishedMenuDto?)null);

        var controller = CreateController();

        var result = await controller.IndexByTenant(_tenantOptions.DefaultTenantId, null, CancellationToken.None);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", view.ViewName);
        var model = Assert.IsType<PublicMenuPageViewModel>(view.Model);
        Assert.False(model.HasMenu);
        Assert.Null(model.Menu);
    }

    private MenusController CreateController()
    {
        return new MenusController(
            NullLogger<MenusController>.Instance,
            _queryHandler.Object,
            Options.Create(_tenantOptions));
    }
}
