namespace EazyMenu.Application.Abstractions.Messaging;

/// <summary>
/// Marker interface for commands that return a result.
/// </summary>
/// <typeparam name="TResponse">Type of result returned by the command.</typeparam>
public interface ICommand<out TResponse>
{
}

/// <summary>
/// Marker interface for commands that don't return a result (void).
/// </summary>
public interface ICommand : ICommand<Unit>
{
}

/// <summary>
/// Represents a void response for commands that don't return a value.
/// </summary>
public readonly record struct Unit
{
    public static Unit Value { get; } = default;
}
