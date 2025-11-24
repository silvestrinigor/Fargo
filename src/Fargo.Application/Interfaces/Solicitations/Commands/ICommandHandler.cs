namespace Fargo.Application.Interfaces.Solicitations.Commands
{
    public interface ICommandHandler<TCommand, TReturn> where TCommand : ICommand<TCommand, TReturn>
    {
        TReturn Handle(TCommand command);
    }
}
