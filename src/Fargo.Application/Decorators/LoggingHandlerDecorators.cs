using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Logging;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Decorators;

/// <summary>
/// Emits the uniform started, completed and failed events, with duration and an actor-correlated
/// scope, via <see cref="HandlerLog"/>. <c>ICurrentActor?</c> is optional: without an actor (the seed
/// worker, login and refresh) only the operation name is attached to the scope.
/// </summary>
internal static class LoggingHandlerDecorator
{
    public static string OperationName<TMessage>() => typeof(TMessage).Name;
}

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
