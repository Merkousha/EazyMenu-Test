using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Features.Menus.Common;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Menus.Commands.UpdateMenuItemPricing;

public sealed class UpdateMenuItemPricingCommandHandler : MenuCommandHandlerBase<UpdateMenuItemPricingCommand, bool>
{
    public UpdateMenuItemPricingCommandHandler(IMenuRepository menuRepository, IUnitOfWork unitOfWork)
        : base(menuRepository, unitOfWork)
    {
    }

    protected override async Task<bool> HandleInternalAsync(UpdateMenuItemPricingCommand command, CancellationToken cancellationToken)
    {
        var menu = await GetMenuAsync(command.TenantId, command.MenuId, cancellationToken);
        var category = GetCategory(menu, command.CategoryId);
        var item = GetItem(category, command.ItemId);

        var currency = string.IsNullOrWhiteSpace(command.Currency) ? Money.DefaultCurrency : command.Currency!;
        var basePrice = Money.From(command.BasePrice, currency);

        var channelPrices = MenuEnumParser.ToChannelPrices(command.ChannelPrices, currency);
        var dictionary = new Dictionary<MenuChannel, Money>(channelPrices);

        menu.UpdateMenuItemPricing(category.Id, item.Id, basePrice, dictionary);

        return await SaveAndReturnAsync(menu, true, cancellationToken);
    }
}
