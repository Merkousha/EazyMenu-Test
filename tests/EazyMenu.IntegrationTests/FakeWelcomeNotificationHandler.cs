using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Features.Notifications.SendWelcomeNotification;

namespace EazyMenu.IntegrationTests;

internal sealed class FakeWelcomeNotificationHandler : ICommandHandler<SendWelcomeNotificationCommand>
{
    public Task<Unit> HandleAsync(SendWelcomeNotificationCommand command, CancellationToken cancellationToken = default)
        => Task.FromResult(Unit.Value);
}
