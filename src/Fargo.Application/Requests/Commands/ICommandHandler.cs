namespace Fargo.Application.Requests.Commands
{
    /// <summary>
    /// Provides a command handler that returns a response.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public interface ICommandHandler<in TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        TResponse Handle(
            TCommand command,
            CancellationToken cancellationToken = default
            );
    }
}
