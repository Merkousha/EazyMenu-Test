using System.Threading;
using System.Threading.Tasks;

namespace EazyMenu.Application.Abstractions.Messaging;

public interface ICommandHandler<TCommand, TResponse> where TCommand : ICommand<TResponse>
{
    Task<TResponse> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

/// <summary>
/// Handler interface for commands that don't return a result.
/// </summary>
public interface ICommandHandler<TCommand> : ICommandHandler<TCommand, Unit> where TCommand : ICommand
{
}
