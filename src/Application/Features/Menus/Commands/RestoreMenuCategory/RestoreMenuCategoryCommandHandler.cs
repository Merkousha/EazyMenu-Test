using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Features.Menus.Common;

namespace EazyMenu.Application.Features.Menus.Commands.RestoreMenuCategory;

public sealed class RestoreMenuCategoryCommandHandler : MenuCommandHandlerBase<RestoreMenuCategoryCommand, bool>
{
    public RestoreMenuCategoryCommandHandler(IMenuRepository menuRepository, IUnitOfWork unitOfWork)
        : base(menuRepository, unitOfWork)
    {
    }

    protected override async Task<bool> HandleInternalAsync(RestoreMenuCategoryCommand command, CancellationToken cancellationToken)
    {
        var menu = await GetMenuAsync(command.TenantId, command.MenuId, cancellationToken);
        var category = GetCategory(menu, command.CategoryId);

        menu.RestoreCategory(category.Id);

        return await SaveAndReturnAsync(menu, true, cancellationToken);
    }
}
