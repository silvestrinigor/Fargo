namespace Fargo.Application.Mediators
{
    public interface IBaseCommand;

    public interface ICommand : IBaseCommand;

    public interface ICommand<out TResponse> { }
}
