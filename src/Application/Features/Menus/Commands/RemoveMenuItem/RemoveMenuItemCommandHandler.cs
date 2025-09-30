using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Features.Menus.Common;

namespace EazyMenu.Application.Features.Menus.Commands.RemoveMenuItem;

public sealed class RemoveMenuItemCommandHandler : MenuCommandHandlerBase<RemoveMenuItemCommand, bool>
{
    public RemoveMenuItemCommandHandler(IMenuRepository menuRepository, IUnitOfWork unitOfWork)
        : base(menuRepository, unitOfWork)
    {
    }

    protected override async Task<bool> HandleInternalAsync(RemoveMenuItemCommand command, CancellationToken cancellationToken)
    {
        var menu = await GetMenuAsync(command.TenantId, command.MenuId, cancellationToken);
        var category = GetCategory(menu, command.CategoryId);
        var item = GetItem(category, command.ItemId);

        menu.RemoveMenuItem(category.Id, item.Id);

        return await SaveAndReturnAsync(menu, true, cancellationToken);
    }
}
