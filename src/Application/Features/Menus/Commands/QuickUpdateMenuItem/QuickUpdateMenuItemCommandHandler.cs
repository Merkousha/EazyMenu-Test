using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Features.Menus.Common;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Menus.Commands.QuickUpdateMenuItem;

public sealed class QuickUpdateMenuItemCommandHandler : MenuCommandHandlerBase<QuickUpdateMenuItemCommand, bool>
{
    public QuickUpdateMenuItemCommandHandler(IMenuRepository menuRepository, IUnitOfWork unitOfWork)
        : base(menuRepository, unitOfWork)
    {
    }

    protected override async Task<bool> HandleInternalAsync(QuickUpdateMenuItemCommand command, CancellationToken cancellationToken)
    {
        var menu = await GetMenuAsync(command.TenantId, command.MenuId, cancellationToken);
        var category = GetCategory(menu, command.CategoryId);
        var item = GetItem(category, command.ItemId);

        var currency = string.IsNullOrWhiteSpace(command.Currency) ? Money.DefaultCurrency : command.Currency!;
        var basePrice = Money.From(command.BasePrice, currency);

        var channelPrices = MenuEnumParser.ToChannelPrices(command.ChannelPrices, currency);
        var priceDictionary = new Dictionary<MenuChannel, Money>(channelPrices);

        menu.UpdateMenuItemPricing(category.Id, item.Id, basePrice, priceDictionary);

        var inventoryState = command.Inventory?.ToInventoryState() ?? InventoryState.Infinite();
        menu.SetMenuItemInventory(category.Id, item.Id, inventoryState);

        menu.SetMenuItemAvailability(category.Id, item.Id, command.IsAvailable);

        return await SaveAndReturnAsync(menu, true, cancellationToken);
    }
}
