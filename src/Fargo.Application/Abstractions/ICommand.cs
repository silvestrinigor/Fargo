namespace Fargo.Application.Abstractions;

public interface ICommand;

public interface ICommand<out TResponse> : ICommand;

/// <summary>
/// Runs the handler inside one explicit database transaction (begin, handler, save, commit, with
/// rollback on failure) instead of the default commit-on-success save.
/// </summary>
/// <remarks>
/// For commands whose handler takes a transaction-scoped lock (such as a PostgreSQL advisory lock)
/// that must be held until the commit. The registration layer detects this marker and selects the
/// transactional decorator, which owns the commit, so the handler must not open its own transaction.
/// </remarks>
public interface ITransactionalCommand : ICommand;

/// <summary>
/// Opt-in for a command that a client may retry safely by sending an idempotency key. A retry with the
/// same key returns the first result instead of running the handler again.
/// </summary>
/// <remarks>
/// The registration layer detects this marker and adds the idempotency decorator, which stores the key in
/// the same transaction as the change. The key identifies the operation: a client that changes the request
/// must use a new key. Never opt in a command whose response carries a secret, because the response is
/// stored to be replayed.
/// </remarks>
public interface IIdempotentCommand : ICommand
{
    /// <summary>
    /// <see langword="null"/> runs the command normally, with no retry protection.
    /// </summary>
    Guid? IdempotencyKey { get; }
}
/// <summary>
/// Exempt from the total <c>AuditLogType</c> mapping. For system or background operations registered
/// outside the audited command pipeline, such as the outbox relay.
/// </summary>
public interface INonAuditedCommand : ICommand;

public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    Task HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default
    );
}

public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    Task<TResponse> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default
    );
}
