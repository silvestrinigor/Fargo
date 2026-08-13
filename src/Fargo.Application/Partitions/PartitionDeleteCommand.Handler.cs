using Fargo.Application.Common;
using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Fargo.Core.Audits;
using Fargo.Core.Entities;
using Fargo.Core.Partitions;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Partitions;

public sealed class PartitionDeleteCommandHandler(
    PartitionService partitionService,
    ActorResolver actorService,
    IPartitionRepository partitionRepository,
    IAuditLogRepository auditLogRepository,
    ICurrentActor currentActor,
    IUnitOfWork unitOfWork,
    ILogger<PartitionDeleteCommandHandler> logger
) : ICommandHandler<PartitionDeleteCommand>
{
    public async Task HandleAsync(
        PartitionDeleteCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.DeleteStarted(command.PartitionGuid, currentActor.Guid);

        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        actor.ThrowIfPermissionDenied(ActionType.DeletePartition);

        var partitionToDelete = await partitionRepository.GetByGuidAsync(command.PartitionGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(partitionToDelete, command.PartitionGuid, EntityType.Partition);

        actor.ThrowIfAccessDenied(partitionToDelete);

        if (partitionToDelete.ParentPartitionGuid is null)
        {
            throw new FargoApplicationException("Cannot delete a partition with no parent partition.");
        }

        var parentPartition = await partitionRepository.GetByGuidAsync(partitionToDelete.ParentPartitionGuid.Value, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(parentPartition, partitionToDelete.ParentPartitionGuid.Value, EntityType.Partition);

        actor.ThrowIfAccessDenied(parentPartition);

        await partitionService.ValidatePartitionCanBeDeletedAsync(partitionToDelete, cancellationToken);

        partitionRepository.Remove(partitionToDelete);

        var audit = AuditLog.CreateAuditLog(actor, partitionToDelete, ActionType.DeletePartition);

        auditLogRepository.Add(audit);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.DeleteCompleted(command.PartitionGuid, currentActor.Guid);
    }
}
