namespace Fargo.Application.Operations;

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

/// <summary>
/// Marks a command whose handler must run inside a single explicit database
/// transaction (begin → handler → save → commit, rolling back on failure)
/// rather than the default commit-on-success autocommit save.
///
/// <para>
/// A command implements this when its handler acquires a transaction-scoped
/// database lock (for example a PostgreSQL advisory lock guarding a
/// read-check-write) that must be held until the commit. The registration layer
/// detects this marker and selects the transactional unit-of-work decorator
/// automatically, so the transactional behaviour cannot be lost by registering
/// the command with the wrong helper.
/// </para>
/// </summary>
public interface ITransactionalCommand : ICommand;
