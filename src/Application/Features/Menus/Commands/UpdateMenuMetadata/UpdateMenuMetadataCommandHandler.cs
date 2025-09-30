using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Features.Menus.Common;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Menus.Commands.UpdateMenuMetadata;

public sealed class UpdateMenuMetadataCommandHandler : MenuCommandHandlerBase<UpdateMenuMetadataCommand, bool>
{
    private readonly IMenuRepository _menuRepository;

    public UpdateMenuMetadataCommandHandler(IMenuRepository menuRepository, IUnitOfWork unitOfWork)
        : base(menuRepository, unitOfWork)
    {
        _menuRepository = menuRepository;
    }

    protected override async Task<bool> HandleInternalAsync(UpdateMenuMetadataCommand command, CancellationToken cancellationToken)
    {
        var menu = await GetMenuAsync(command.TenantId, command.MenuId, cancellationToken);

        if (command.Name is null || command.Name.Count == 0)
        {
            throw new BusinessRuleViolationException("نام منو الزامی است.");
        }

        var name = LocalizedTextMapper.ToLocalizedText(command.Name);
        menu.Rename(name);

        LocalizedText? description = null;
        if (command.Description is not null && command.Description.Count > 0)
        {
            description = LocalizedTextMapper.ToLocalizedText(command.Description);
        }

        menu.UpdateDescription(description);

        if (command.IsDefault)
        {
            menu.MarkAsDefault();
            if (!TenantId.TryCreate(command.TenantId, out var tenantId))
            {
                throw new BusinessRuleViolationException("شناسه مستاجر معتبر نیست.");
            }

            var menus = await _menuRepository.GetByTenantAsync(tenantId, cancellationToken);
            foreach (var other in menus.Where(m => m.Id != menu.Id && m.IsDefault))
            {
                other.UnsetDefault();
            }
        }
        else if (menu.IsDefault)
        {
            menu.UnsetDefault();
        }

        return await SaveAndReturnAsync(menu, true, cancellationToken);
    }
}
