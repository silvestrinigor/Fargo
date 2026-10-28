using Fargo.Application.Common;
using Fargo.Core.Actors;
using Fargo.Core.Audits;
using Fargo.Core.Entities;
using Fargo.Core.Partitions;

namespace Fargo.Application.Partitions;

public sealed class PartitionUpdateCommandHandler(
    ActorResolver actorService,
    PartitionService partitionService,
    IPartitionRepository partitionRepository,
    IAuditLogRepository auditLogRepository,
    IUnitOfWork unitOfWork,
    ICurrentActor currentActor
) : ICommandHandler<PartitionUpdateCommand>
{
    public async Task HandleAsync(PartitionUpdateCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.EditPartition);

        var partitionToEdit = await partitionRepository.GetByGuidAsync(command.PartitionGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(partitionToEdit, command.PartitionGuid, EntityType.Partition);

        actor.ThrowIfAccessDenied(partitionToEdit);

        var partitionAudit = AuditLog.CreateAuditLog(actor, partitionToEdit, AuditLogType.EntityUpdated);

        if (command.Update.Name is { } name)
        {
            partitionToEdit.Name = name;

            partitionAudit.Metadata.AddName(partitionToEdit.Name);
        }

        if (command.Update.Description is { } description)
        {
            partitionToEdit.Description = description;

            partitionAudit.Metadata.AddDescription(partitionToEdit.Description);
        }

        if (command.Update.ParentPartitionGuid is { } parentPartitionGuidToSet)
        {
            var parentPartitionToSet = await partitionRepository.GetByGuidAsync(parentPartitionGuidToSet, cancellationToken);

            EntityNotFoundFargoApplicationException.ThrowIfNull(parentPartitionToSet, parentPartitionGuidToSet, EntityType.Partition);

            actor.ThrowIfAccessDenied(parentPartitionToSet);

            await partitionService.ValidateParentPartitionHierarchyAssignmentAsync(parentPartitionToSet, partitionToEdit, cancellationToken);

            partitionToEdit.SetParentPartition(parentPartitionToSet);

            partitionAudit.Metadata.AddParentPartition(partitionToEdit.ParentPartitionGuid);
        }

        auditLogRepository.Add(partitionAudit);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
