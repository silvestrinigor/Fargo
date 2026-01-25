namespace Fargo.Application.Requests.Commands
{
    /// <summary>
    /// Provides an a command interface that does not return a response.
    /// </summary>
    public interface ICommand;

    /// <summary>
    /// Provides an a command interface that returns a response.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public interface ICommand<out TResponse> { }
}
