namespace Fargo.Application;

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
public interface ICommand<out TResponse> : ICommand;
