using Fargo.Application.Common;
using Fargo.Core.Actors;
using Fargo.Core.Audits;
using Fargo.Core.Entities;
using Fargo.Core.Informations;
using Fargo.Core.ServiceAccounts;

namespace Fargo.Application.ServiceAccounts;

public sealed class ServiceAccountUpdateCommandHandler(
    ActorResolver actorService,
    IServiceAccountRepository serviceAccountRepository,
    IAuditLogRepository auditLogRepository,
    ICurrentActor currentActor
) : ICommandHandler<ServiceAccountUpdateCommand>
{
    public async Task HandleAsync(ServiceAccountUpdateCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.EditServiceAccount);

        var account = await serviceAccountRepository.GetByGuidAsync(command.ServiceAccountGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(account, command.ServiceAccountGuid, EntityType.ServiceAccount);

        if (command.Update.Description is { } description)
        {
            account.Description = new Description(description);
        }

        if (command.Update.IsActive is { } isActive)
        {
            if (isActive)
            {
                account.Activate();
            }
            else
            {
                account.Deactivate();
            }
        }

        if (command.Update.PermissionsToAdd is { } permissionsToAdd)
        {
            foreach (var permission in permissionsToAdd)
            {
                account.AddPermission(permission);
            }
        }

        if (command.Update.PermissionsToRemove is { } permissionsToRemove)
        {
            foreach (var permission in permissionsToRemove)
            {
                account.RemovePermission(permission);
            }
        }

        if (command.Update.PartitionAccessesToAdd is { } partitionsToAdd)
        {
            foreach (var partitionGuid in partitionsToAdd)
            {
                account.AddPartitionAccess(partitionGuid);
            }
        }

        if (command.Update.PartitionAccessesToRemove is { } partitionsToRemove)
        {
            foreach (var partitionGuid in partitionsToRemove)
            {
                account.RemovePartitionAccess(partitionGuid);
            }
        }

        var audit = AuditLog.CreateAuditLogForGlobalEntity(actor, account, AuditLogType.EntityUpdated);

        auditLogRepository.Add(audit);
    }
}
