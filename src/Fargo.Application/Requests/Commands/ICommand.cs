namespace Fargo.Application.Requests.Commands
{
    public interface ICommand;

    public interface ICommand<out TResponse>;

    public interface ICommandHandler<in TCommand>
        where TCommand : ICommand
    {
        Task Handle(
            TCommand command,
            CancellationToken cancellationToken = default
            );
    }

    public interface ICommandHandler<in TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        Task<TResponse> Handle(
            TCommand command,
            CancellationToken cancellationToken = default
            );
    }
}