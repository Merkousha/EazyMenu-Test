using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Features.Menus.Common;

namespace EazyMenu.Application.Features.Menus.Commands.UpdateMenuCategory;

public sealed class UpdateMenuCategoryCommandHandler : MenuCommandHandlerBase<UpdateMenuCategoryCommand, bool>
{
    public UpdateMenuCategoryCommandHandler(IMenuRepository menuRepository, IUnitOfWork unitOfWork)
        : base(menuRepository, unitOfWork)
    {
    }

    protected override async Task<bool> HandleInternalAsync(UpdateMenuCategoryCommand command, CancellationToken cancellationToken)
    {
        var menu = await GetMenuAsync(command.TenantId, command.MenuId, cancellationToken);
        var category = GetCategory(menu, command.CategoryId);

        if (command.Name is null || command.Name.Count == 0)
        {
            throw new BusinessRuleViolationException("نام دسته‌بندی الزامی است.");
        }

        var name = LocalizedTextMapper.ToLocalizedText(command.Name);
        menu.UpdateCategory(category.Id, name, command.DisplayOrder, command.IconUrl);

        return await SaveAndReturnAsync(menu, true, cancellationToken);
    }
}
