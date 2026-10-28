using Fargo.Application.Common;
using Fargo.Core.Actors;
using Fargo.Core.Audits;
using Fargo.Core.Common;
using Fargo.Core.Entities;
using Fargo.Core.Informations;
using Fargo.Core.Partitions;

namespace Fargo.Application.Partitions;

public sealed class PartitionCreateCommandHandler(
    ActorResolver actorService,
    IPartitionRepository partitionRepository,
    IAuditLogRepository auditLogRepository,
    ICurrentActor currentActor
) : ICommandHandler<PartitionCreateCommand, Guid>
{
    public async Task<Guid> HandleAsync(
        PartitionCreateCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.CreatePartition);

        // When the caller does not specify a parent, default to the global
        // partition. The actor must have access to the global partition to use
        // this default; otherwise they must name a parent they can access.
        var parentPartitionGuid =
            command.Create.ParentPartitionGuid ?? FargoCoreWellKnowGuids.GlobalPartitionGuid;

        var parentPartition = await partitionRepository.GetByGuidAsync(parentPartitionGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(parentPartition, parentPartitionGuid, EntityType.Partition);

        if (command.Create.ParentPartitionGuid is null && !actor.HasAccess(parentPartition))
        {
            throw new FargoApplicationException(
                "No parent partition was provided and the default global partition is not "
                + "accessible to you. Specify a parent partition you have access to.",
                FargoErrorType.AccessDenied);
        }

        actor.ThrowIfAccessDenied(parentPartition);

        var newPartition = Partition.CreatePartition(command.Create.Name, parentPartition);

        var partitionAudit = AuditLog.CreateAuditLog(actor, newPartition, AuditLogType.EntityCreated);

        partitionAudit.Metadata.AddName(newPartition.Name);

        partitionAudit.Metadata.AddParentPartition(newPartition.ParentPartitionGuid);

        newPartition.Description = command.Create.Description ?? Description.Empty;

        partitionAudit.Metadata.AddDescription(newPartition.Description);

        auditLogRepository.Add(partitionAudit);

        partitionRepository.Add(newPartition);

        return newPartition.Guid;
    }
}
