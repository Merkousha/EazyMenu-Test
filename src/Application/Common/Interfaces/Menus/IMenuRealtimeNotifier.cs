using System;
using System.Threading;
using System.Threading.Tasks;

namespace EazyMenu.Application.Common.Interfaces.Menus;

public interface IMenuRealtimeNotifier
{
    Task PublishMenuChangedAsync(MenuRealtimeNotification notification, CancellationToken cancellationToken = default);
}

public sealed record MenuRealtimeNotification(
    Guid TenantId,
    Guid MenuId,
    string ChangeType,
    Guid? CategoryId = null,
    Guid? ItemId = null,
    int? PublishedVersion = null);
