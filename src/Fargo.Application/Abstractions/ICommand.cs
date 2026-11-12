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
