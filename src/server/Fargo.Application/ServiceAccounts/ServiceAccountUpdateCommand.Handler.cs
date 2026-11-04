using Fargo.Application.Common;
using Fargo.Core.Audits;
using Fargo.Core.Entities;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.ServiceAccounts;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Informations;

namespace Fargo.Application.ServiceAccounts;

public sealed class ServiceAccountUpdateCommandHandler(
    ActorResolver actorService,
    IServiceAccountRepository serviceAccountRepository,
    IPartitionRepository partitionRepository,
    IAuditLogRepository auditLogRepository,
    ICurrentActor currentActor
) : ICommandHandler<ServiceAccountUpdateCommand>
{
    public async Task HandleAsync(ServiceAccountUpdateCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.EditServiceAccount);

        var account = await serviceAccountRepository.GetByGuidAsync(command.ServiceAccountGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(account, command.ServiceAccountGuid, EntityType.Actor);

        var accountActor = account.Actor;

        var audit = accountActor.PartitionGuids.Count > 0
            ? AuditLog.CreateAuditLog(actor, accountActor, AuditLogType.EntityUpdated)
            : AuditLog.CreateAuditLogForGlobalEntity(actor, accountActor, AuditLogType.EntityUpdated);

        if (command.Update.Description is { } description)
        {
            accountActor.Description = new Description(description);

            audit.Metadata.AddDescription(accountActor.Description);
        }

        if (command.Update.IsActive is { } isActive)
        {
            if (isActive)
            {
                accountActor.Activate();
            }
            else
            {
                accountActor.Deactivate();
                account.RotateAuthVersion();
            }

            audit.Metadata.AddIsActive(accountActor.IsActive);
        }

        if (command.Update.PermissionsToAdd is { } permissionsToAdd)
        {
            foreach (var permission in permissionsToAdd)
            {
                actor.ThrowIfPermissionDenied(permission);

                accountActor.AddPermission(permission);
            }

            audit.Metadata.AddPermissionsAdded([.. permissionsToAdd]);
        }

        if (command.Update.PermissionsToRemove is { } permissionsToRemove)
        {
            foreach (var permission in permissionsToRemove)
            {
                actor.ThrowIfPermissionDenied(permission);

                accountActor.RemovePermission(permission);
            }

            audit.Metadata.AddPermissionsRemoved([.. permissionsToRemove]);
        }

        if (command.Update.PartitionsToAdd is { } partitionMembershipsToAdd)
        {
            foreach (var partitionGuid in partitionMembershipsToAdd)
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid);

                actor.ThrowIfAccessDenied(partition);

                accountActor.AddPartition(partition);
            }

            audit.Metadata.AddPartitionsAdded([.. partitionMembershipsToAdd]);
        }

        if (command.Update.PartitionsToRemove is { } partitionMembershipsToRemove)
        {
            foreach (var partitionGuid in partitionMembershipsToRemove)
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid);

                actor.ThrowIfAccessDenied(partition);

                accountActor.RemovePartition(partition.Guid);
            }

            audit.Metadata.AddPartitionsRemoved([.. partitionMembershipsToRemove]);
        }

        if (command.Update.PartitionAccessesToAdd is { } partitionAccessesToAdd)
        {
            foreach (var partitionGuid in partitionAccessesToAdd)
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid);

                actor.ThrowIfAccessDenied(partition);

                accountActor.AddPartitionAccess(partition);
            }

            audit.Metadata.AddPartitionAccessesAdded([.. partitionAccessesToAdd]);
        }

        if (command.Update.PartitionAccessesToRemove is { } partitionAccessesToRemove)
        {
            foreach (var partitionGuid in partitionAccessesToRemove)
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid);

                actor.ThrowIfAccessDenied(partition);

                accountActor.RemovePartitionAccess(partition.Guid);
            }

            audit.Metadata.AddPartitionAccessesRemoved([.. partitionAccessesToRemove]);
        }

        auditLogRepository.Add(audit);
    }
}
