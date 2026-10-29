using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Fargo.Application.Common;

/// <summary>
/// Shared, source-generated logging primitives for application handlers.
///
/// <para>
/// Handlers no longer log their own start/finish. The
/// <see cref="LoggingCommandHandlerDecorator{TCommand}"/> /
/// <see cref="LoggingQueryHandlerDecorator{TQuery, TResponse}"/> wrap every
/// handler and use <see cref="BeginOperation"/> to emit a consistent
/// <em>started</em> event, a terminal <em>completed</em> (with elapsed
/// milliseconds) or <em>failed</em> (with elapsed milliseconds and the
/// exception) event, and to attach the operation name — and, when available,
/// the current actor's identity — as a logging scope so every nested log entry
/// is correlated.
/// </para>
/// </summary>
public static partial class HandlerLog
{
    /// <summary>
    /// Begins an operation: opens a correlation scope (operation name plus, when
    /// supplied, the actor identity) and logs the <em>started</em> event. Call
    /// <see cref="OperationLog.Complete"/> on success or
    /// <see cref="OperationLog.Fail"/> on error; disposing the returned handle
    /// emits the terminal event with the measured duration.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="operation">
    /// A stable operation label derived from the message type (e.g.
    /// "ArticleDeleteCommand").
    /// </param>
    /// <param name="actor">
    /// The current actor, or <see langword="null"/> for actor-less operations
    /// (e.g. login, system initialization) or hosts without an actor context.
    /// </param>
    public static OperationLog BeginOperation(this ILogger logger, string operation, ICurrentActor? actor = null)
        => new(logger, operation, actor);

    [LoggerMessage(Level = LogLevel.Information, Message = "{Operation} started.")]
    internal static partial void OperationStarted(this ILogger logger, string operation);

    [LoggerMessage(Level = LogLevel.Information, Message = "{Operation} completed in {ElapsedMilliseconds} ms.")]
    internal static partial void OperationCompleted(this ILogger logger, string operation, long elapsedMilliseconds);

    [LoggerMessage(Level = LogLevel.Error, Message = "{Operation} failed after {ElapsedMilliseconds} ms.")]
    internal static partial void OperationFailed(this ILogger logger, string operation, long elapsedMilliseconds, Exception exception);
}

/// <summary>
/// Tracks a single handler invocation: holds the correlation scope and a
/// stopwatch, and on <see cref="Dispose"/> emits the terminal log event.
///
/// <para>
/// Call <see cref="Complete"/> on the success path or <see cref="Fail"/> with
/// the thrown exception on the error path. If the handle is disposed without
/// either having been called, the operation is still recorded as failed (with
/// no exception) so a terminal event is never missing.
/// </para>
/// </summary>
public sealed class OperationLog : IDisposable
{
    private readonly ILogger logger;
    private readonly string operation;
    private readonly IDisposable? scope;
    private readonly long startTimestamp;
    private bool completed;
    private Exception? failure;
    private bool disposed;

    internal OperationLog(ILogger logger, string operation, ICurrentActor? actor)
    {
        this.logger = logger;
        this.operation = operation;

        scope = logger.BeginScope(BuildScopeState(operation, actor));
        startTimestamp = Stopwatch.GetTimestamp();

        logger.OperationStarted(operation);
    }

    /// <summary>Marks the operation as successful.</summary>
    public void Complete() => completed = true;

    /// <summary>Marks the operation as failed with the given exception.</summary>
    public void Fail(Exception exception) => failure = exception;

    /// <inheritdoc />
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;

        var elapsed = (long)Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;

        if (completed && failure is null)
        {
            logger.OperationCompleted(operation, elapsed);
        }
        else
        {
            logger.OperationFailed(operation, elapsed, failure!);
        }

        scope?.Dispose();
    }

    private static Dictionary<string, object> BuildScopeState(string operation, ICurrentActor? actor)
    {
        var state = new Dictionary<string, object> { ["Operation"] = operation };

        if (actor is { IsAuthenticated: true })
        {
            state["ActorGuid"] = actor.Guid;
            state["ActorType"] = actor.ActorType;
        }

        return state;
    }
}
