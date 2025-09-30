using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Features.Menus.Common;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Menus.Commands.AddMenuItem;

public sealed class AddMenuItemCommandHandler : MenuCommandHandlerBase<AddMenuItemCommand, Guid>
{
    public AddMenuItemCommandHandler(IMenuRepository menuRepository, IUnitOfWork unitOfWork)
        : base(menuRepository, unitOfWork)
    {
    }

    protected override async Task<Guid> HandleInternalAsync(AddMenuItemCommand command, CancellationToken cancellationToken)
    {
        var menu = await GetMenuAsync(command.TenantId, command.MenuId, cancellationToken);
        var category = GetCategory(menu, command.CategoryId);

        if (command.Name is null || command.Name.Count == 0)
        {
            throw new BusinessRuleViolationException("نام آیتم الزامی است.");
        }

        var name = LocalizedTextMapper.ToLocalizedText(command.Name);
        LocalizedText? description = null;
        if (command.Description is not null && command.Description.Count > 0)
        {
            description = LocalizedTextMapper.ToLocalizedText(command.Description);
        }

        var currency = string.IsNullOrWhiteSpace(command.Currency) ? Money.DefaultCurrency : command.Currency!;
        var basePrice = Money.From(command.BasePrice, currency);

        var inventory = command.Inventory?.ToInventoryState() ?? InventoryState.Infinite();

        var channelPrices = MenuEnumParser.ToChannelPrices(command.ChannelPrices, currency);
        var tags = MenuEnumParser.ToTags(command.Tags);

        var dictionary = new Dictionary<MenuChannel, Money>(channelPrices);
        var item = menu.AddItem(category.Id, name, basePrice, description, command.IsAvailable, inventory, command.ImageUrl, dictionary, tags);

        return await SaveAndReturnAsync(menu, item.Id.Value, cancellationToken);
    }
}
