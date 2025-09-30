using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Features.Menus.Commands.QuickUpdateMenuItem;
using EazyMenu.Application.Features.Menus.Common;
using EazyMenu.Domain.Aggregates.Menus;
using EazyMenu.Domain.ValueObjects;
using Moq;
using Xunit;

namespace EazyMenu.UnitTests.Application.Features.Menus.Commands.QuickUpdateMenuItem;

public sealed class QuickUpdateMenuItemCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldUpdatePricingInventoryAndAvailability()
    {
        var menu = Menu.Create(TenantId.New(), LocalizedText.Create("منوی اصلی"));
        var category = menu.AddCategory(LocalizedText.Create("غذاهای اصلی"));
        var item = menu.AddItem(category.Id, LocalizedText.Create("کوبیده"), Money.From(150_000m));

        var repository = new Mock<IMenuRepository>();
        repository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<TenantId>(), It.IsAny<MenuId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(menu);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new QuickUpdateMenuItemCommandHandler(repository.Object, unitOfWork.Object);

        var command = new QuickUpdateMenuItemCommand(
            menu.TenantId.Value,
            menu.Id.Value,
            category.Id.Value,
            item.Id.Value,
            BasePrice: 195_000m,
            Currency: "IRT",
            ChannelPrices: new Dictionary<string, decimal> { ["Delivery"] = 205_000m },
            Inventory: new InventoryPayload("Track", 3, 1),
            IsAvailable: true);

        await handler.HandleAsync(command, CancellationToken.None);

        repository.Verify(repo => repo.UpdateAsync(menu, It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        Assert.Equal(195_000m, item.BasePrice.Amount);
        Assert.Equal(InventoryTrackingMode.Track, item.Inventory.Mode);
        Assert.Equal(3, item.Inventory.Quantity);
        Assert.True(item.IsAvailable);
        Assert.True(item.ChannelPrices.ContainsKey(MenuChannel.Delivery));
    }
}
