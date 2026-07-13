using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Fargo.Core.Partitions;
using Fargo.Core.Shared;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Partitions;

public sealed class PartitionDeleteCommandHandler(
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

        ActorNotFoundFargoException.ThrowIfNull(actor, currentActor.ActorId);

        actor.ThrowIfPermissionDenied(ActionType.DeletePartition);

        var partition = await partitionRepository.GetByGuidAsync(command.PartitionGuid, cancellationToken);

        EntityNotFoundFargoException.ThrowIfNull(partition, command.PartitionGuid, EntityType.Partition);

        actor.ThrowIfAccessDeniedToPartition(partition);

        partitionRepository.Remove(partition);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.DeleteCompleted(command.PartitionGuid, currentActor.ActorId);
    }
}
