using Fargo.Application.Abstractions;
using Fargo.Application.Auditing;
using Fargo.Application.Common.Actors;
using Fargo.Core.Audits;

namespace Fargo.Application.Decorators;

/// <summary>
/// Records an <see cref="AuditLog"/> for each command, only after the inner handler succeeds and only
/// when an authenticated actor is present. Actor-less contexts (the seed worker, an unauthenticated
/// login) record nothing, so <c>ICurrentActor?</c> is optional.
/// </summary>
/// <remarks>
/// Sits inside the unit-of-work decorator, so the audit log is saved by the same save or transaction
/// as the command's own changes.
/// </remarks>
internal static class AuditCommandHandlerDecorator
{
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
