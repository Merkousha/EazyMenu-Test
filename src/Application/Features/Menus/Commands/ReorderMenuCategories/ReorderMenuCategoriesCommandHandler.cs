using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Features.Menus.Common;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Menus.Commands.ReorderMenuCategories;

public sealed class ReorderMenuCategoriesCommandHandler : MenuCommandHandlerBase<ReorderMenuCategoriesCommand, bool>
{
    public ReorderMenuCategoriesCommandHandler(IMenuRepository menuRepository, IUnitOfWork unitOfWork)
        : base(menuRepository, unitOfWork)
    {
    }

    protected override async Task<bool> HandleInternalAsync(ReorderMenuCategoriesCommand command, CancellationToken cancellationToken)
    {
        var menu = await GetMenuAsync(command.TenantId, command.MenuId, cancellationToken);

        var ids = command.OrderedCategoryIds?.ToList() ?? new List<Guid>();
        if (ids.Count == 0)
        {
            throw new BusinessRuleViolationException("لیست دسته‌ها نمی‌تواند خالی باشد.");
        }

        var converted = ids.Select(id =>
        {
            if (!MenuCategoryId.TryCreate(id, out var categoryId))
            {
                throw new BusinessRuleViolationException("شناسه دسته منو معتبر نیست.");
            }

            return categoryId;
        }).ToList();

        menu.ReorderCategories(converted);

        return await SaveAndReturnAsync(menu, true, cancellationToken);
    }
}
