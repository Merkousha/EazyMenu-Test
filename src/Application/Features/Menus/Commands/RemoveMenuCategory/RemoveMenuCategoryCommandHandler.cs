using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Features.Menus.Common;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Menus.Commands.RemoveMenuCategory;

public sealed class RemoveMenuCategoryCommandHandler : MenuCommandHandlerBase<RemoveMenuCategoryCommand, bool>
{
    public RemoveMenuCategoryCommandHandler(IMenuRepository menuRepository, IUnitOfWork unitOfWork)
        : base(menuRepository, unitOfWork)
    {
    }

    protected override async Task<bool> HandleInternalAsync(RemoveMenuCategoryCommand command, CancellationToken cancellationToken)
    {
        var menu = await GetMenuAsync(command.TenantId, command.MenuId, cancellationToken);

        if (!MenuCategoryId.TryCreate(command.CategoryId, out var categoryId))
        {
            throw new BusinessRuleViolationException("شناسه دسته منو معتبر نیست.");
        }

        menu.RemoveCategory(categoryId);

        return await SaveAndReturnAsync(menu, true, cancellationToken);
    }
}
