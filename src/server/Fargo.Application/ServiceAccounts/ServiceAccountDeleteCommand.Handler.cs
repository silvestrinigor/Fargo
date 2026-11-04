using Fargo.Application.Common;
using Fargo.Core.Actors;
using Fargo.Core.Audits;
using Fargo.Core.Entities;
using Fargo.Core.ServiceAccounts;

namespace Fargo.Application.ServiceAccounts;

public sealed class ServiceAccountDeleteCommandHandler(
    ActorResolver actorService,
    IServiceAccountRepository serviceAccountRepository,
    IAuditLogRepository auditLogRepository,
    ICurrentActor currentActor
) : ICommandHandler<ServiceAccountDeleteCommand>
{
    public async Task HandleAsync(ServiceAccountDeleteCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.DeleteServiceAccount);

        var account = await serviceAccountRepository.GetByGuidAsync(command.ServiceAccountGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(account, command.ServiceAccountGuid, EntityType.Actor);

        var audit = AuditLog.CreateAuditLogForGlobalEntity(actor, account.Actor, AuditLogType.EntityDeleted);

        serviceAccountRepository.Remove(account);

        auditLogRepository.Add(audit);
    }
}
