namespace Fargo.Application.Common;

/// <summary>
/// Commit-on-success unit-of-work decorators for command handlers. These
/// decorators wrap the real handler and, once it completes normally, commit the
/// pending changes via <see cref="IUnitOfWork.SaveChangesAsync"/>, so command
/// handlers no longer perform their own terminal success save.
///
/// <para>
/// The commit is intentionally skipped when the inner handler throws: the
/// exception is allowed to propagate and no <see cref="IUnitOfWork.SaveChangesAsync"/>
/// call is made, leaving any in-flight changes uncommitted. Handlers that must
/// persist on a reject path (for example the login/refresh flows that record a
/// failed attempt before throwing) continue to call
/// <see cref="IUnitOfWork.SaveChangesAsync"/> themselves.
/// </para>
/// </summary>
/// <remarks>Commit-on-success unit-of-work decorator for a command handler with no response.</remarks>
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
/// Transactional unit-of-work decorator for a no-response command handler.
/// Unlike <see cref="UnitOfWorkCommandHandlerDecorator{TCommand}"/>, which saves
/// once on success in autocommit mode, this decorator runs the inner handler
/// inside a single explicit database transaction (begin → handler → save →
/// commit, rolling back on failure).
///
/// <para>
/// It is used for commands whose handler acquires a transaction-scoped database
/// lock (for example a PostgreSQL advisory lock guarding a read-check-write such
/// as the item containment-cycle check): the lock must be held until the commit,
/// which requires the save to happen inside the same transaction the handler
/// runs in. The handler therefore must NOT open its own transaction — this
/// decorator owns the single commit for the command.
/// </para>
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

/// <summary>Commit-on-success unit-of-work decorator for a command handler that returns a response.</summary>
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
