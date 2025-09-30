using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Features.Menus.Commands.AddMenuCategory;
using EazyMenu.Application.Features.Menus.Commands.ArchiveMenuCategory;
using EazyMenu.Application.Features.Menus.Commands.ReorderMenuCategories;
using EazyMenu.Application.Features.Menus.Commands.RestoreMenuCategory;
using EazyMenu.Application.Features.Menus.Commands.UpdateMenuCategory;
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

public sealed class MenuCategoriesControllerTests
{
    private readonly Mock<IDashboardTenantProvider> _tenantProvider = new();
    private readonly Mock<IQueryHandler<GetMenuDetailsQuery, MenuDetailsDto>> _detailsHandler = new();
    private readonly Mock<ICommandHandler<AddMenuCategoryCommand, Guid>> _addHandler = new();
    private readonly Mock<ICommandHandler<UpdateMenuCategoryCommand, bool>> _updateHandler = new();
    private readonly Mock<ICommandHandler<ArchiveMenuCategoryCommand, bool>> _archiveHandler = new();
    private readonly Mock<ICommandHandler<RestoreMenuCategoryCommand, bool>> _restoreHandler = new();
    private readonly Mock<ICommandHandler<ReorderMenuCategoriesCommand, bool>> _reorderHandler = new();

    private readonly MenuDetailsDto _sampleDetails;

    public MenuCategoriesControllerTests()
    {
        _sampleDetails = MenuDetailsDtoFactory.Create();
        _tenantProvider
            .Setup(provider => provider.GetActiveTenantIdAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());
        _detailsHandler
            .Setup(handler => handler.HandleAsync(It.IsAny<GetMenuDetailsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_sampleDetails);
    }

    [Fact]
    public async Task Create_WhenSuccessful_ReturnsPartialView()
    {
        var controller = CreateController();
        var input = new CreateMenuCategoryInput
        {
            Name = new MenuLocalizedTextInput { Fa = "دسته تست" },
            IconUrl = "https://example.com/icon.png"
        };

        var result = await controller.Create(Guid.NewGuid(), input, CancellationToken.None);

        var partial = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("~/Views/Menus/Partials/_CategoryList.cshtml", partial.ViewName);
        var model = Assert.IsAssignableFrom<IReadOnlyCollection<MenuCategoryViewModel>>(partial.Model);
        Assert.NotEmpty(model);
    }

    [Fact]
    public async Task Create_WhenTenantMissing_ReturnsBadRequest()
    {
        _tenantProvider.Setup(provider => provider.GetActiveTenantIdAsync(It.IsAny<CancellationToken>())).ReturnsAsync((Guid?)null);
        var controller = CreateController();

        var result = await controller.Create(Guid.NewGuid(), new CreateMenuCategoryInput(), CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("مستاجر فعال", badRequest.Value!.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Update_WhenCommandFails_ReturnsErrorResult()
    {
        _updateHandler
            .Setup(handler => handler.HandleAsync(It.IsAny<UpdateMenuCategoryCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleViolationException("نام دسته تکراری است."));

        var controller = CreateController();

        var result = await controller.Update(Guid.NewGuid(), Guid.NewGuid(), new UpdateMenuCategoryInput { Name = new MenuLocalizedTextInput { Fa = "دسته" } }, CancellationToken.None);

    var errorResult = Assert.IsType<UnprocessableEntityObjectResult>(result);
    Assert.Equal(422, errorResult.StatusCode);
    }

    [Fact]
    public async Task Reorder_WhenEmptyIds_ReturnsValidationProblem()
    {
        var controller = CreateController();

    var result = await controller.Reorder(Guid.NewGuid(), new ReorderCategoriesInput(), CancellationToken.None);

    var validation = Assert.IsType<ObjectResult>(result);
    var details = Assert.IsType<ValidationProblemDetails>(validation.Value);
    Assert.Contains(nameof(ReorderCategoriesInput.CategoryIds), details.Errors.Keys);
    }

    private MenuCategoriesController CreateController()
    {
        return new MenuCategoriesController(
            NullLogger<MenuCategoriesController>.Instance,
            _tenantProvider.Object,
            _detailsHandler.Object,
            _addHandler.Object,
            _updateHandler.Object,
            _archiveHandler.Object,
            _restoreHandler.Object,
            _reorderHandler.Object);
    }

    private static class MenuDetailsDtoFactory
    {
        public static MenuDetailsDto Create()
        {
            var category = new MenuCategoryDetailsDto(
                Guid.NewGuid(),
                new Dictionary<string, string> { ["fa-IR"] = "غذاهای اصلی" },
                DisplayOrder: 1,
                IconUrl: null,
                IsArchived: false,
                Items: Array.Empty<MenuItemDetailsDto>());

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
