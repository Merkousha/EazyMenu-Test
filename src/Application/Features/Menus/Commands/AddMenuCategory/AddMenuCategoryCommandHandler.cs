using System;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Features.Menus.Common;

namespace EazyMenu.Application.Features.Menus.Commands.AddMenuCategory;

public sealed class AddMenuCategoryCommandHandler : MenuCommandHandlerBase<AddMenuCategoryCommand, Guid>
{
    public AddMenuCategoryCommandHandler(IMenuRepository menuRepository, IUnitOfWork unitOfWork)
        : base(menuRepository, unitOfWork)
    {
    }

    protected override async Task<Guid> HandleInternalAsync(AddMenuCategoryCommand command, CancellationToken cancellationToken)
    {
        var menu = await GetMenuAsync(command.TenantId, command.MenuId, cancellationToken);

        if (command.Name is null || command.Name.Count == 0)
        {
            throw new BusinessRuleViolationException("نام دسته‌بندی الزامی است.");
        }

        var name = LocalizedTextMapper.ToLocalizedText(command.Name);
        var category = menu.AddCategory(name, command.IconUrl, command.DisplayOrder);

        return await SaveAndReturnAsync(menu, category.Id.Value, cancellationToken);
    }
}
