namespace Fargo.Application.Requests.Commands
{
    public interface IBaseCommand;

    public interface ICommand : IBaseCommand;

    public interface ICommand<out TResponse> { }
}
