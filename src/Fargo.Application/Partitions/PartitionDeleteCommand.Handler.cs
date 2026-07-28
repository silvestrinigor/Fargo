using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Fargo.Core.Partitions;
using Fargo.Core.Shared;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Partitions;

public sealed class PartitionDeleteCommandHandler(
    PartitionService partitionService,
    ActorService actorService,
    IPartitionRepository partitionRepository,
    ICurrentActor currentActor,
    IUnitOfWork unitOfWork,
    ILogger<PartitionDeleteCommandHandler> logger
) : ICommandHandler<PartitionDeleteCommand>
{
    public async Task HandleAsync(
        PartitionDeleteCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.DeleteStarted(command.PartitionGuid, currentActor.ActorId);

        var actor = await actorService.GetActorByActorIdAsync(currentActor.ActorId, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.ActorId);

        actor.ThrowIfPermissionDenied(ActionType.DeletePartition);

        var partitionToDelete = await partitionRepository.GetByGuidAsync(command.PartitionGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(partitionToDelete, command.PartitionGuid, EntityType.Partition);

        actor.ThrowIfAccessDenied(partitionToDelete);

        if (!partitionToDelete.HasParentPartition)
        {
            throw new FargoApplicationException(
                "Cannot delete a partition with no parent partition.",
                FargoApplicationErrorType.CannotDeletePartitionWithNotParentPartition);
        }

        var parentPartition = await partitionRepository.GetByGuidAsync(partitionToDelete.ParentPartitionGuid.Value, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(parentPartition, partitionToDelete.ParentPartitionGuid.Value, EntityType.Partition);

        actor.ThrowIfAccessDenied(parentPartition);

        await partitionService.ValidatePartitionDelete(partitionToDelete, cancellationToken);

        partitionRepository.Remove(partitionToDelete);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.DeleteCompleted(command.PartitionGuid, currentActor.ActorId);
    }
}
