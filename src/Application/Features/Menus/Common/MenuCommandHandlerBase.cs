using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Domain.Aggregates.Menus;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Menus.Common;

public abstract class MenuCommandHandlerBase<TCommand, TResult> : ICommandHandler<TCommand, TResult> where TCommand : ICommand<TResult>
{
    private readonly IMenuRepository _menuRepository;
    private readonly IUnitOfWork _unitOfWork;

    protected MenuCommandHandlerBase(IMenuRepository menuRepository, IUnitOfWork unitOfWork)
    {
        _menuRepository = menuRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        return await HandleInternalAsync(command, cancellationToken);
    }

    protected async Task<Menu> GetMenuAsync(Guid tenantId, Guid menuId, CancellationToken cancellationToken)
    {
        if (!TenantId.TryCreate(tenantId, out var tid))
        {
            throw new BusinessRuleViolationException("شناسه مستاجر معتبر نیست.");
        }

        if (!MenuId.TryCreate(menuId, out var mid))
        {
            throw new BusinessRuleViolationException("شناسه منو معتبر نیست.");
        }

        var menu = await _menuRepository.GetByIdAsync(tid, mid, cancellationToken);
        if (menu is null)
        {
            throw new NotFoundException("منوی مورد نظر یافت نشد.");
        }

        return menu;
    }

    protected static MenuCategory GetCategory(Menu menu, Guid categoryId)
    {
        if (!MenuCategoryId.TryCreate(categoryId, out var cid))
        {
            throw new BusinessRuleViolationException("شناسه دسته منو معتبر نیست.");
        }

        return menu.Categories.FirstOrDefault(c => c.Id == cid) ?? throw new NotFoundException("دسته مورد نظر یافت نشد.");
    }

    protected static MenuItem GetItem(MenuCategory category, Guid itemId)
    {
        if (!MenuItemId.TryCreate(itemId, out var iid))
        {
            throw new BusinessRuleViolationException("شناسه آیتم منو معتبر نیست.");
        }

        return category.Items.FirstOrDefault(i => i.Id == iid) ?? throw new NotFoundException("آیتم مورد نظر یافت نشد.");
    }

    protected async Task SaveAsync(Menu menu, CancellationToken cancellationToken)
    {
        await _menuRepository.UpdateAsync(menu, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    protected async Task<TResult> SaveAndReturnAsync(Menu menu, TResult result, CancellationToken cancellationToken)
    {
        await SaveAsync(menu, cancellationToken);
        return result;
    }

    protected abstract Task<TResult> HandleInternalAsync(TCommand command, CancellationToken cancellationToken);
}
