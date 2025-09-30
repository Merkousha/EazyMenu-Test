using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Features.Menus.Common;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Menus.Commands.ReorderMenuItems;

public sealed class ReorderMenuItemsCommandHandler : MenuCommandHandlerBase<ReorderMenuItemsCommand, bool>
{
    public ReorderMenuItemsCommandHandler(IMenuRepository menuRepository, IUnitOfWork unitOfWork)
        : base(menuRepository, unitOfWork)
    {
    }

    protected override async Task<bool> HandleInternalAsync(ReorderMenuItemsCommand command, CancellationToken cancellationToken)
    {
        if (command.OrderedItemIds is null || command.OrderedItemIds.Count == 0)
        {
            throw new BusinessRuleViolationException("لیست آیتم‌ها نمی‌تواند خالی باشد.");
        }

        var menu = await GetMenuAsync(command.TenantId, command.MenuId, cancellationToken);
        var category = GetCategory(menu, command.CategoryId);

        var orderedIds = new List<MenuItemId>(command.OrderedItemIds.Count);
        var seen = new HashSet<Guid>();

        foreach (var rawId in command.OrderedItemIds)
        {
            if (!MenuItemId.TryCreate(rawId, out var itemId))
            {
                throw new BusinessRuleViolationException("شناسه آیتم منو معتبر نیست.");
            }

            if (!seen.Add(rawId))
            {
                throw new BusinessRuleViolationException("شناسه آیتم‌ها نباید تکراری باشند.");
            }

            orderedIds.Add(itemId);
        }

        menu.ReorderMenuItems(category.Id, orderedIds);

        return await SaveAndReturnAsync(menu, true, cancellationToken);
    }
}
