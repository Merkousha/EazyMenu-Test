using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Common.Interfaces.Menus;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Features.Menus.Commands.AddMenuItem;
using EazyMenu.Application.Features.Menus.Commands.RemoveMenuItem;
using EazyMenu.Application.Features.Menus.Commands.ReorderMenuItems;
using EazyMenu.Application.Features.Menus.Commands.SetMenuItemAvailability;
using EazyMenu.Application.Features.Menus.Commands.UpdateMenuItemDetails;
using EazyMenu.Application.Features.Menus.Commands.UpdateMenuItemPricing;
using EazyMenu.Application.Features.Menus.Commands.QuickUpdateMenuItem;
using EazyMenu.Application.Features.Menus.Common;
using EazyMenu.Application.Features.Menus.Queries.GetMenuDetails;
using EazyMenu.Web.Controllers;
using EazyMenu.Web.Models.Menus;
using EazyMenu.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace EazyMenu.UnitTests.Presentation.Web.Controllers;

public sealed class MenuItemsControllerTests
{
    private readonly Mock<IDashboardTenantProvider> _tenantProvider = new();
    private readonly Mock<IQueryHandler<GetMenuDetailsQuery, MenuDetailsDto>> _detailsHandler = new();
    private readonly Mock<ICommandHandler<AddMenuItemCommand, Guid>> _addHandler = new();
    private readonly Mock<ICommandHandler<UpdateMenuItemDetailsCommand, bool>> _updateDetailsHandler = new();
    private readonly Mock<ICommandHandler<UpdateMenuItemPricingCommand, bool>> _updatePricingHandler = new();
    private readonly Mock<ICommandHandler<SetMenuItemAvailabilityCommand, bool>> _availabilityHandler = new();
    private readonly Mock<ICommandHandler<QuickUpdateMenuItemCommand, bool>> _quickUpdateHandler = new();
    private readonly Mock<ICommandHandler<RemoveMenuItemCommand, bool>> _removeHandler = new();
    private readonly Mock<ICommandHandler<ReorderMenuItemsCommand, bool>> _reorderHandler = new();
    private readonly Mock<IMenuRealtimeNotifier> _menuRealtimeNotifier = new();

    private readonly MenuDetailsDto _sampleDetails;

    public MenuItemsControllerTests()
    {
        _sampleDetails = MenuDetailsDtoFactory.Create();
        _tenantProvider.Setup(provider => provider.GetActiveTenantIdAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Guid.NewGuid());
        _detailsHandler
            .Setup(handler => handler.HandleAsync(It.IsAny<GetMenuDetailsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_sampleDetails);
        _quickUpdateHandler
            .Setup(handler => handler.HandleAsync(It.IsAny<QuickUpdateMenuItemCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _menuRealtimeNotifier
            .Setup(notifier => notifier.PublishMenuChangedAsync(It.IsAny<MenuRealtimeNotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Create_WhenValid_ReturnsPartialView()
    {
        var controller = CreateController();
        var input = new CreateMenuItemInput
        {
            Name = new MenuLocalizedTextInput { Fa = "پیتزا" },
            BasePrice = 120000,
            Inventory = new InventoryInput { Mode = "Infinite" }
        };

        var result = await controller.Create(Guid.NewGuid(), Guid.NewGuid(), input, CancellationToken.None);

        var partial = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("~/Views/Menus/Partials/_CategoryList.cshtml", partial.ViewName);
    }

    [Fact]
    public async Task Update_WhenCommandThrows_ReturnsErrorResult()
    {
        _updateDetailsHandler
            .Setup(handler => handler.HandleAsync(It.IsAny<UpdateMenuItemDetailsCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleViolationException("جزئیات نامعتبر"));

        var controller = CreateController();
        var input = new UpdateMenuItemInput
        {
            Name = new MenuLocalizedTextInput { Fa = "پاستا" },
            BasePrice = 150000,
            ChannelPrices = new ChannelPriceInput()
        };

        var result = await controller.Update(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), input, CancellationToken.None);

        var errorResult = Assert.IsType<UnprocessableEntityObjectResult>(result);
        Assert.Equal(422, errorResult.StatusCode);
    }

    [Fact]
    public async Task SetAvailability_WhenMissingTenant_ReturnsBadRequest()
    {
        _tenantProvider.Setup(provider => provider.GetActiveTenantIdAsync(It.IsAny<CancellationToken>())).ReturnsAsync((Guid?)null);
        var controller = CreateController();

        var result = await controller.SetAvailability(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new SetMenuItemAvailabilityInput { IsAvailable = true }, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Reorder_WhenInvalidIds_ReturnsValidationProblem()
    {
        var controller = CreateController();

        var result = await controller.Reorder(Guid.NewGuid(), Guid.NewGuid(), new ReorderItemsInput(), CancellationToken.None);

        var validation = Assert.IsType<ObjectResult>(result);
        var details = Assert.IsType<ValidationProblemDetails>(validation.Value);
        Assert.Contains(nameof(ReorderItemsInput.ItemIds), details.Errors.Keys);
    }

    [Fact]
    public async Task QuickUpdate_WhenValid_ReturnsQuickUpdatePartial()
    {
        var controller = CreateController();

        var input = new QuickUpdateMenuItemInput
        {
            BasePrice = 180000m,
            Inventory = new InventoryInput { Mode = "Infinite" },
            ChannelPrices = new ChannelPriceInput()
        };

        var result = await controller.QuickUpdate(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), input, CancellationToken.None);

        var partial = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("~/Views/Menus/Partials/_QuickUpdateTable.cshtml", partial.ViewName);
    }

    private MenuItemsController CreateController()
    {
        return new MenuItemsController(
            NullLogger<MenuItemsController>.Instance,
            _tenantProvider.Object,
            _detailsHandler.Object,
            _addHandler.Object,
            _updateDetailsHandler.Object,
            _updatePricingHandler.Object,
            _availabilityHandler.Object,
            _quickUpdateHandler.Object,
            _removeHandler.Object,
        _reorderHandler.Object,
        _menuRealtimeNotifier.Object);
    }

    private static class MenuDetailsDtoFactory
    {
        public static MenuDetailsDto Create()
        {
            var item = new MenuItemDetailsDto(
                Guid.NewGuid(),
                new Dictionary<string, string> { ["fa-IR"] = "پاستا" },
                Description: null,
                BasePrice: 100000,
                Currency: "IRT",
                IsAvailable: true,
                Inventory: new InventoryDetailsDto("Infinite", null, null, false),
                ImageUrl: null,
                DisplayOrder: 1,
                ChannelPrices: new Dictionary<string, decimal>(),
                Tags: Array.Empty<string>());

            var category = new MenuCategoryDetailsDto(
                Guid.NewGuid(),
                new Dictionary<string, string> { ["fa-IR"] = "خوراک" },
                DisplayOrder: 1,
                IconUrl: null,
                IsArchived: false,
                Items: new[] { item });

            return new MenuDetailsDto(
                Guid.NewGuid(),
                new Dictionary<string, string> { ["fa-IR"] = "منوی اصلی" },
                Description: null,
                IsDefault: true,
                IsArchived: false,
                PublishedVersion: 1,
                CreatedAtUtc: DateTime.UtcNow,
                UpdatedAtUtc: DateTime.UtcNow,
                Categories: new[] { category });
        }
    }
}
