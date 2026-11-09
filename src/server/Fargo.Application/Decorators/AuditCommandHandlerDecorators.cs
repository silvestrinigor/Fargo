using Fargo.Application.Abstractions;
using Fargo.Application.Auditing;
using Fargo.Application.Common.Actors;
using Fargo.Core.Audits;


namespace Fargo.Application.Decorators;

/// <summary>
/// Cross-cutting audit logging for command handlers. These decorators record an
/// <see cref="AuditLog"/> describing the command an actor executed, so handlers
/// themselves contain no audit code.
///
/// <para>
/// The audit is recorded only when the inner handler completes successfully, and
/// only when an authenticated actor is present. In actor-less contexts (such as
/// the seed worker running <c>InitializeSystemCommand</c>, or an unauthenticated
/// login attempt) there is no acting actor to attribute the command to, so no
/// audit is recorded. The current actor is therefore an optional dependency
/// (<c>ICurrentActor?</c>), supplied as <see langword="null"/> by the
/// registration layer when the service is not registered.
/// </para>
///
/// <para>
/// These decorators sit inside the unit-of-work decorator in the handler
/// pipeline, so the added audit log is persisted by the same save/transaction
/// that commits the command's own changes.
/// </para>
/// </summary>
internal static class AuditCommandHandlerDecorator
{
    /// <summary>
    /// Records an audit log for the executed command when an authenticated actor
    /// is present.
    /// </summary>
    public static void Record<TCommand>(
        TCommand command,
        ICurrentActor? currentActor,
        IAuditLogRepository auditLogRepository)
        where TCommand : ICommand
    {
        if (currentActor is null || !currentActor.IsAuthenticated)
        {
            return;
        }

        var auditLog = AuditLog.ForCommand(
            currentActor.Guid,
            currentActor.ActorType,
            AuditLogTypeResolver.Resolve(typeof(TCommand)),
            typeof(TCommand).Name,
            CommandAuditMetadataSerializer.Serialize(command!));

        auditLogRepository.Add(auditLog);
    }
}

/// <summary>Audit decorator for a command handler with no response.</summary>
public sealed class AuditCommandHandlerDecorator<TCommand>(
    ICommandHandler<TCommand> inner,
    ICurrentActor? currentActor,
    IAuditLogRepository auditLogRepository) : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    public async Task HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        await inner.HandleAsync(command, cancellationToken);

        AuditCommandHandlerDecorator.Record(command, currentActor, auditLogRepository);
    }
}

/// <summary>Audit decorator for a command handler that returns a response.</summary>
public sealed class AuditCommandHandlerDecorator<TCommand, TResponse>(
    ICommandHandler<TCommand, TResponse> inner,
    ICurrentActor? currentActor,
    IAuditLogRepository auditLogRepository) : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    public async Task<TResponse> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        var response = await inner.HandleAsync(command, cancellationToken);

        AuditCommandHandlerDecorator.Record(command, currentActor, auditLogRepository);

        return response;
    }
}
