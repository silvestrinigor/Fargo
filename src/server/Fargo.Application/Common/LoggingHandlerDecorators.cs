using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Common;

/// <summary>
/// Cross-cutting logging for command/query handlers. These decorators wrap the
/// real handler and emit the uniform started / completed / failed events (with
/// duration and an actor-correlated scope) via <see cref="HandlerLog"/>, so
/// handlers themselves contain no start/finish logging.
///
/// <para>
/// The current actor is resolved optionally from the request scope: it is
/// present in the HTTP host (where <c>ICurrentActor</c> is registered and the
/// request is authenticated) and absent in actor-less contexts such as the seed
/// worker or the login/refresh flows, in which case only the operation name is
/// attached to the scope.
/// </para>
/// </summary>
internal static class LoggingHandlerDecorator
{
    /// <summary>
    /// Derives a stable operation label from the message type (its simple name,
    /// e.g. <c>ArticleDeleteCommand</c>).
    /// </summary>
    public static string OperationName<TMessage>() => typeof(TMessage).Name;

    public static ICurrentActor? TryGetActor(IServiceProvider services)
        => services.GetService<ICurrentActor>();
}

/// <summary>Logging decorator for a command handler with no response.</summary>
public sealed class LoggingCommandHandlerDecorator<TCommand>(
    ICommandHandler<TCommand> inner,
    IServiceProvider services,
    ILogger<TCommand> logger) : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    public async Task HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        using var op = logger.BeginOperation(
            LoggingHandlerDecorator.OperationName<TCommand>(),
            LoggingHandlerDecorator.TryGetActor(services));

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
    IServiceProvider services,
    ILogger<TCommand> logger) : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    public async Task<TResponse> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        using var op = logger.BeginOperation(
            LoggingHandlerDecorator.OperationName<TCommand>(),
            LoggingHandlerDecorator.TryGetActor(services));

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
    IServiceProvider services,
    ILogger<TQuery> logger) : IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    public async Task<TResponse> HandleAsync(TQuery query, CancellationToken cancellationToken = default)
    {
        using var op = logger.BeginOperation(
            LoggingHandlerDecorator.OperationName<TQuery>(),
            LoggingHandlerDecorator.TryGetActor(services));

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
