using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Features.Menus.Common;

namespace EazyMenu.Application.Features.Menus.Commands.AdjustMenuItemInventory;

public sealed class AdjustMenuItemInventoryCommandHandler : MenuCommandHandlerBase<AdjustMenuItemInventoryCommand, bool>
{
    public AdjustMenuItemInventoryCommandHandler(IMenuRepository menuRepository, IUnitOfWork unitOfWork)
        : base(menuRepository, unitOfWork)
    {
    }

    protected override async Task<bool> HandleInternalAsync(AdjustMenuItemInventoryCommand command, CancellationToken cancellationToken)
    {
        var menu = await GetMenuAsync(command.TenantId, command.MenuId, cancellationToken);
        var category = GetCategory(menu, command.CategoryId);
        var item = GetItem(category, command.ItemId);

        menu.AdjustMenuItemInventory(category.Id, item.Id, command.Delta);

        return await SaveAndReturnAsync(menu, true, cancellationToken);
    }
}
