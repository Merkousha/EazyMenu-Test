namespace EazyMenu.Application.Abstractions.Messaging;

/// <summary>
/// Marker interface for commands that return a result.
/// </summary>
/// <typeparam name="TResponse">Type of result returned by the command.</typeparam>
public interface ICommand<out TResponse>
{
}
