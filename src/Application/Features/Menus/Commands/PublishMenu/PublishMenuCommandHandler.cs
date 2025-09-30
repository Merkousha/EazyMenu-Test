using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Features.Menus.Common;

namespace EazyMenu.Application.Features.Menus.Commands.PublishMenu;

public sealed class PublishMenuCommandHandler : MenuCommandHandlerBase<PublishMenuCommand, int>
{
    public PublishMenuCommandHandler(IMenuRepository menuRepository, IUnitOfWork unitOfWork)
        : base(menuRepository, unitOfWork)
    {
    }

    protected override async Task<int> HandleInternalAsync(PublishMenuCommand command, CancellationToken cancellationToken)
    {
        var menu = await GetMenuAsync(command.TenantId, command.MenuId, cancellationToken);

        menu.PublishNextVersion();

        return await SaveAndReturnAsync(menu, menu.PublishedVersion, cancellationToken);
    }
}
