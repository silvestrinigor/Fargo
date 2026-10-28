using Fargo.Application.Common;
using Fargo.Core.Actors;
using Fargo.Core.Audits;
using Fargo.Core.Entities;
using Fargo.Core.Partitions;

namespace Fargo.Application.Partitions;

public sealed class PartitionDeleteCommandHandler(
    PartitionService partitionService,
    ActorResolver actorService,
    IPartitionRepository partitionRepository,
    IAuditLogRepository auditLogRepository,
    ICurrentActor currentActor,
    IUnitOfWork unitOfWork
) : ICommandHandler<PartitionDeleteCommand>
{
    public async Task HandleAsync(
        PartitionDeleteCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

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

        var audit = AuditLog.CreateAuditLog(actor, partitionToDelete, AuditLogType.EntityDeleted);

        auditLogRepository.Add(audit);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
