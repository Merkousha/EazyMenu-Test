using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Features.Menus.Common;
using EazyMenu.Domain.Aggregates.Menus;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Menus.Commands.CreateMenu;

public sealed class CreateMenuCommandHandler : ICommandHandler<CreateMenuCommand, Guid>
{
    private readonly IMenuRepository _menuRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateMenuCommandHandler(IMenuRepository menuRepository, IUnitOfWork unitOfWork)
    {
        _menuRepository = menuRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> HandleAsync(CreateMenuCommand command, CancellationToken cancellationToken = default)
    {
        if (!TenantId.TryCreate(command.TenantId, out var tenantId))
        {
            throw new BusinessRuleViolationException("شناسه مستاجر معتبر نیست.");
        }

        if (command.Name is null || command.Name.Count == 0)
        {
            throw new BusinessRuleViolationException("نام منو الزامی است.");
        }

        var name = LocalizedTextMapper.ToLocalizedText(command.Name);
        var description = command.Description is null || command.Description.Count == 0
            ? null
            : LocalizedTextMapper.ToLocalizedText(command.Description);

        var menu = Menu.Create(tenantId, name, description, command.IsDefault);

        var existingMenus = await _menuRepository.GetByTenantAsync(tenantId, cancellationToken);
        if (command.IsDefault)
        {
            foreach (var other in existingMenus.Where(m => m.IsDefault))
            {
                other.UnsetDefault();
                await _menuRepository.UpdateAsync(other, cancellationToken);
            }
        }

        await _menuRepository.AddAsync(menu, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return menu.Id.Value;
    }
}
