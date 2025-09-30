using System;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Features.Menus.Common;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Menus.Commands.UpdateMenuItemDetails;

public sealed class UpdateMenuItemDetailsCommandHandler : MenuCommandHandlerBase<UpdateMenuItemDetailsCommand, bool>
{
    public UpdateMenuItemDetailsCommandHandler(IMenuRepository menuRepository, IUnitOfWork unitOfWork)
        : base(menuRepository, unitOfWork)
    {
    }

    protected override async Task<bool> HandleInternalAsync(UpdateMenuItemDetailsCommand command, CancellationToken cancellationToken)
    {
        var menu = await GetMenuAsync(command.TenantId, command.MenuId, cancellationToken);
        var category = GetCategory(menu, command.CategoryId);
        var item = GetItem(category, command.ItemId);

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

        menu.UpdateMenuItemDetails(category.Id, item.Id, name, description, command.ImageUrl);

        if (command.Tags is not null)
        {
            var tags = MenuEnumParser.ToTags(command.Tags);
            menu.UpdateMenuItemTags(category.Id, item.Id, tags);
        }

        return await SaveAndReturnAsync(menu, true, cancellationToken);
    }
}
