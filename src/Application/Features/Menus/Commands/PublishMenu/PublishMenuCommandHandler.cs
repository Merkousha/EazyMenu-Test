using System;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Interfaces.Menus;
using EazyMenu.Application.Features.Menus.Common;

namespace EazyMenu.Application.Features.Menus.Commands.PublishMenu;

public sealed class PublishMenuCommandHandler : MenuCommandHandlerBase<PublishMenuCommand, int>
{
    private readonly IMenuPublicationWriter _publicationWriter;
    private readonly IMenuRealtimeNotifier _realtimeNotifier;

    public PublishMenuCommandHandler(
        IMenuRepository menuRepository,
        IUnitOfWork unitOfWork,
        IMenuPublicationWriter publicationWriter,
        IMenuRealtimeNotifier realtimeNotifier)
        : base(menuRepository, unitOfWork)
    {
        _publicationWriter = publicationWriter;
        _realtimeNotifier = realtimeNotifier;
    }

    protected override async Task<int> HandleInternalAsync(PublishMenuCommand command, CancellationToken cancellationToken)
    {
        var menu = await GetMenuAsync(command.TenantId, command.MenuId, cancellationToken);

        menu.PublishNextVersion();

        var publishedAtUtc = DateTime.UtcNow;
        var snapshot = MenuPublicationFactory.CreateSnapshot(menu, publishedAtUtc);
        await _publicationWriter.SaveAsync(snapshot, cancellationToken);

        var version = await SaveAndReturnAsync(menu, menu.PublishedVersion, cancellationToken);

        await _realtimeNotifier.PublishMenuChangedAsync(
            new MenuRealtimeNotification(command.TenantId, command.MenuId, "menu-published", PublishedVersion: version),
            cancellationToken);

        return version;
    }
}
