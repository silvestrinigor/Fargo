namespace Fargo.Application.Requests.Commands;

/// <summary>
/// Represents a command in the application layer.
///
/// Commands represent operations that change the state of the system
/// and do not return a result.
/// </summary>
public interface ICommand;

/// <summary>
/// Represents a command that returns a response.
/// </summary>
/// <typeparam name="TResponse">
/// The type of response returned by the command.
/// </typeparam>
public interface ICommand<out TResponse>;

/// <summary>
/// Defines a handler responsible for executing a command.
/// </summary>
/// <typeparam name="TCommand">
/// The type of command handled by this handler.
/// </typeparam>
public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    /// <summary>
    /// Executes the specified command.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Handle(
        TCommand command,
        CancellationToken cancellationToken = default
    );
}

/// <summary>
/// Defines a handler responsible for executing a command that produces a response.
/// </summary>
/// <typeparam name="TCommand">
/// The type of command handled by this handler.
/// </typeparam>
/// <typeparam name="TResponse">
/// The type of response returned by the command.
/// </typeparam>
public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    /// <summary>
    /// Executes the specified command.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The response produced by the command.</returns>
    Task<TResponse> Handle(
        TCommand command,
        CancellationToken cancellationToken = default
    );
}
