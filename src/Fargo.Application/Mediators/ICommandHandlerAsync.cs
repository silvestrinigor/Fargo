namespace Fargo.Application.Mediators
{
    public interface ICommandHandlerAsync<in TCommand>
        where TCommand : ICommand
    {
        Task HandleAsync(
            TCommand command,
            CancellationToken cancellationToken = default);
    }

    public interface ICommandHandlerAsync<in TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        Task<TResponse> HandleAsync(
            TCommand command,
            CancellationToken cancellationToken = default);
    }
}
