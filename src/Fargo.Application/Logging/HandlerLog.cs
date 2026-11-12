using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Core.Common;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Fargo.Application.Logging;

/// <remarks>
/// The command and query logging decorators wrap every handler and use <see cref="BeginOperation"/> so
/// each operation emits a started event and a terminal completed or failed event with the elapsed
/// milliseconds. The operation name and, when available, the actor identity are attached as a logging
/// scope so nested log entries are correlated.
/// </remarks>
public static partial class HandlerLog
{
    /// <summary>
    /// Opens a correlation scope and logs the started event. Call <see cref="OperationLog.Complete"/> or
    /// <see cref="OperationLog.Fail"/>; disposing the handle emits the terminal event.
    /// </summary>
    /// <param name="actor">
    /// Null for actor-less operations (login, system initialization) or hosts without an actor context.
    /// </param>
    public static OperationLog BeginOperation(this ILogger logger, string operation, ICurrentActor? actor = null)
        => new(logger, operation, actor);

    [LoggerMessage(Level = LogLevel.Information, Message = "{Operation} started.")]
    internal static partial void OperationStarted(this ILogger logger, string operation);

    [LoggerMessage(Level = LogLevel.Information, Message = "{Operation} completed in {ElapsedMilliseconds} ms.")]
    internal static partial void OperationCompleted(this ILogger logger, string operation, long elapsedMilliseconds);

    [LoggerMessage(Level = LogLevel.Error, Message = "{Operation} failed after {ElapsedMilliseconds} ms.")]
    internal static partial void OperationFailed(this ILogger logger, string operation, long elapsedMilliseconds, Exception exception);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "{Operation} rejected after {ElapsedMilliseconds} ms: {ExceptionType} ({ErrorType}): {Reason}")]
    internal static partial void OperationRejected(
        this ILogger logger, string operation, long elapsedMilliseconds, string exceptionType, FargoErrorType errorType, string reason);
}

/// <remarks>
/// If the handle is disposed without <see cref="Complete"/> or <see cref="Fail"/>, the operation is still
/// recorded as failed (with no exception), so a terminal event is never missing.
/// </remarks>
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

    public void Complete() => completed = true;

    public void Fail(Exception exception) => failure = exception;

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
        else if (TryGetExpectedFailure(failure, out var errorType))
        {
            // Expected outcomes (mapped to HTTP responses by the exception handlers): warn, no stack trace.
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.OperationRejected(
                    operation, elapsed, failure!.GetType().Name, errorType, GetReason(failure));
            }
        }
        else
        {
            logger.OperationFailed(operation, elapsed, failure!);
        }

        scope?.Dispose();
    }

    // The internal reason (never sent to the client) wins over the public message.
    private static string GetReason(Exception exception)
        => (exception as FargoApplicationException)?.InternalReason ?? exception.Message;

    private static bool TryGetExpectedFailure(Exception? exception, out FargoErrorType errorType)
    {
        switch (exception)
        {
            case FargoApplicationException applicationException:
                errorType = applicationException.ErrorType;
                return true;
            case FargoCoreException coreException:
                errorType = coreException.ErrorType;
                return true;
            default:
                errorType = FargoErrorType.None;
                return false;
        }
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
