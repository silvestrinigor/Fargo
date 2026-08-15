using Fargo.Application.Common;
using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Fargo.Core.Audits;
using Fargo.Core.Entities;
using Fargo.Core.Partitions;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Partitions;

public sealed class PartitionUpdateCommandHandler(
    ActorResolver actorService,
    PartitionService partitionService,
    IPartitionRepository partitionRepository,
    IAuditLogRepository auditLogRepository,
    IUnitOfWork unitOfWork,
    ICurrentActor currentActor,
    ILogger<PartitionUpdateCommandHandler> logger
) : ICommandHandler<PartitionUpdateCommand>
{
    public async Task HandleAsync(PartitionUpdateCommand command, CancellationToken cancellationToken = default)
    {
        logger.UpdateStarted(command.PartitionGuid, currentActor.Guid);

        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        actor.ThrowIfPermissionDenied(ActionType.EditPartition);

        var partitionToEdit = await partitionRepository.GetByGuidAsync(command.PartitionGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(partitionToEdit, command.PartitionGuid, EntityType.Partition);

        actor.ThrowIfAccessDenied(partitionToEdit);

        var partitionAudit = AuditLog.CreateAuditLog(actor, partitionToEdit, ActionType.EditPartition);

        partitionToEdit.Name = command.Update.Name ?? partitionToEdit.Name;

        partitionToEdit.Description = command.Update.Description ?? partitionToEdit.Description;

        if (command.Update.ParentPartitionGuid is { } parentPartitionGuidToSet)
        {
            var parentPartitionToSet = await partitionRepository.GetByGuidAsync(parentPartitionGuidToSet, cancellationToken);

            EntityNotFoundFargoApplicationException.ThrowIfNull(parentPartitionToSet, parentPartitionGuidToSet, EntityType.Partition);

            actor.ThrowIfAccessDenied(parentPartitionToSet);

            await partitionService.ValidateParentPartitionHierarchyAssignmentAsync(parentPartitionToSet, partitionToEdit, cancellationToken);

            partitionToEdit.SetParentPartition(parentPartitionToSet);
        }

        auditLogRepository.Add(partitionAudit);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.UpdateCompleted(partitionToEdit.Guid, currentActor.Guid);
    }
}
