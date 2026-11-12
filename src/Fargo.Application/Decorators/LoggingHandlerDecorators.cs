using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Logging;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Decorators;

/// <summary>
/// Cross-cutting logging for command/query handlers. These decorators wrap the
/// real handler and emit the uniform started / completed / failed events (with
/// duration and an actor-correlated scope) via <see cref="HandlerLog"/>, so
/// handlers themselves contain no start/finish logging.
///
/// <para>
/// The current actor is an optional dependency (<c>ICurrentActor?</c>): it is
/// present in the HTTP host (where <c>ICurrentActor</c> is registered and the
/// request is authenticated) and absent in actor-less contexts such as the seed
/// worker or the login/refresh flows, in which case only the operation name is
/// attached to the scope. The registration layer supplies <see langword="null"/>
/// when the service is not registered, so the decorator depends on the actor
/// abstraction directly instead of reaching into the service provider.
/// </para>
/// </summary>
internal static class LoggingHandlerDecorator
{
    /// <summary>
    /// Derives a stable operation label from the message type (its simple name,
    /// e.g. <c>ArticleDeleteCommand</c>).
    /// </summary>
    public static string OperationName<TMessage>() => typeof(TMessage).Name;
}

/// <summary>Logging decorator for a command handler with no response.</summary>
public sealed class LoggingCommandHandlerDecorator<TCommand>(
    ICommandHandler<TCommand> inner,
    ICurrentActor? currentActor,
    ILogger<TCommand> logger) : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    public async Task HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        using var op = logger.BeginOperation(
            LoggingHandlerDecorator.OperationName<TCommand>(),
            currentActor);

        try
        {
            await inner.HandleAsync(command, cancellationToken);
            op.Complete();
        }
        catch (Exception ex)
        {
            op.Fail(ex);
            throw;
        }
    }
}

/// <summary>Logging decorator for a command handler that returns a response.</summary>
public sealed class LoggingCommandHandlerDecorator<TCommand, TResponse>(
    ICommandHandler<TCommand, TResponse> inner,
    ICurrentActor? currentActor,
    ILogger<TCommand> logger) : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    public async Task<TResponse> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        using var op = logger.BeginOperation(
            LoggingHandlerDecorator.OperationName<TCommand>(),
            currentActor);

        try
        {
            var response = await inner.HandleAsync(command, cancellationToken);
            op.Complete();
            return response;
        }
        catch (Exception ex)
        {
            op.Fail(ex);
            throw;
        }
    }
}

/// <summary>Logging decorator for a query handler.</summary>
public sealed class LoggingQueryHandlerDecorator<TQuery, TResponse>(
    IQueryHandler<TQuery, TResponse> inner,
    ICurrentActor? currentActor,
    ILogger<TQuery> logger) : IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    public async Task<TResponse> HandleAsync(TQuery query, CancellationToken cancellationToken = default)
    {
        using var op = logger.BeginOperation(
            LoggingHandlerDecorator.OperationName<TQuery>(),
            currentActor);

        try
        {
            var response = await inner.HandleAsync(query, cancellationToken);
            op.Complete();
            return response;
        }
        catch (Exception ex)
        {
            op.Fail(ex);
            throw;
        }
    }
}
