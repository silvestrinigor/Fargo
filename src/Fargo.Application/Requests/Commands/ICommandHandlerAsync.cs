namespace Fargo.Application.Requests.Commands
{
    /// <summary>
    /// Provides a command handler that does not return a response.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command.</typeparam>
    public interface ICommandHandlerAsync<in TCommand>
        where TCommand : ICommand
    {
        Task HandleAsync(
            TCommand command,
            CancellationToken cancellationToken = default
            );
    }

    /// <summary>
    /// Provides a command handler that returns a response.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public interface ICommandHandlerAsync<in TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        Task<TResponse> HandleAsync(
            TCommand command,
            CancellationToken cancellationToken = default
            );
    }
}
