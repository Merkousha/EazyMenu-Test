using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Features.Menus.Common;

namespace EazyMenu.Application.Features.Menus.Commands.ArchiveMenuCategory;

public sealed class ArchiveMenuCategoryCommandHandler : MenuCommandHandlerBase<ArchiveMenuCategoryCommand, bool>
{
    public ArchiveMenuCategoryCommandHandler(IMenuRepository menuRepository, IUnitOfWork unitOfWork)
        : base(menuRepository, unitOfWork)
    {
    }

    protected override async Task<bool> HandleInternalAsync(ArchiveMenuCategoryCommand command, CancellationToken cancellationToken)
    {
        var menu = await GetMenuAsync(command.TenantId, command.MenuId, cancellationToken);
        var category = GetCategory(menu, command.CategoryId);

        menu.ArchiveCategory(category.Id);

        return await SaveAndReturnAsync(menu, true, cancellationToken);
    }
}
