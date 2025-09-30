using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Features.Menus.Common;

namespace EazyMenu.Application.Features.Menus.Commands.SetMenuItemAvailability;

public sealed class SetMenuItemAvailabilityCommandHandler : MenuCommandHandlerBase<SetMenuItemAvailabilityCommand, bool>
{
    public SetMenuItemAvailabilityCommandHandler(IMenuRepository menuRepository, IUnitOfWork unitOfWork)
        : base(menuRepository, unitOfWork)
    {
    }

    protected override async Task<bool> HandleInternalAsync(SetMenuItemAvailabilityCommand command, CancellationToken cancellationToken)
    {
        var menu = await GetMenuAsync(command.TenantId, command.MenuId, cancellationToken);
        var category = GetCategory(menu, command.CategoryId);
        var item = GetItem(category, command.ItemId);

        menu.SetMenuItemAvailability(category.Id, item.Id, command.IsAvailable);

        return await SaveAndReturnAsync(menu, true, cancellationToken);
    }
}
