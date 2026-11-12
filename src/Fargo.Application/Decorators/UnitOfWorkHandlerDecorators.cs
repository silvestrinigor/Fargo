using Fargo.Application.Abstractions;
using Fargo.Application.Persistence;

namespace Fargo.Application.Decorators;

/// <summary>
/// Commits the pending changes via <see cref="IUnitOfWork.SaveChangesAsync"/> once the handler
/// completes. Nothing is saved when the handler throws, so a handler that must persist on a reject
/// path (the login and refresh flows record a failed attempt) calls
/// <see cref="IUnitOfWork.SaveChangesAsync"/> itself before throwing.
/// </summary>
public sealed class UnitOfWorkCommandHandlerDecorator<TCommand>(
    ICommandHandler<TCommand> inner,
    IUnitOfWork unitOfWork) : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    public async Task HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        await inner.HandleAsync(command, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// Runs the handler and the save in one explicit transaction, for <see cref="ITransactionalCommand"/>
/// commands that hold a transaction-scoped lock until the commit. The handler must not open its own
/// transaction: this decorator owns the single commit.
/// </summary>
public sealed class TransactionalCommandHandlerDecorator<TCommand>(
    ICommandHandler<TCommand> inner,
    IUnitOfWork unitOfWork) : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    public async Task HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        await unitOfWork.ExecuteInTransactionAsync(
            ct => inner.HandleAsync(command, ct),
            cancellationToken);
    }
}

public sealed class UnitOfWorkCommandHandlerDecorator<TCommand, TResponse>(
    ICommandHandler<TCommand, TResponse> inner,
    IUnitOfWork unitOfWork) : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    public async Task<TResponse> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        var response = await inner.HandleAsync(command, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return response;
    }
}
