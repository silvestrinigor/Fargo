using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Fargo.Core.Partitions;
using Fargo.Core.Shared;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Partitions;

public sealed class PartitionUpdateCommandHandler(
    ActorService actorService,
    IPartitionRepository partitionRepository,
    IUnitOfWork unitOfWork,
    ICurrentActor currentActor,
    ILogger<PartitionUpdateCommandHandler> logger
) : ICommandHandler<PartitionUpdateCommand>
{
    public async Task HandleAsync(
        PartitionUpdateCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.UpdateStarted(command.PartitionGuid, currentActor.ActorId);

        var actor = await actorService.GetActorByActorIdAsync(currentActor.ActorId, cancellationToken);

        ActorAssertFound.ThrowNotFoundIfNull(actor, currentActor.ActorId);

        actor.ThrowIfPermissionDenied(ActionType.EditPartition);

        var partition = await partitionRepository.GetByGuidAsync(command.PartitionGuid, cancellationToken);

        EntityAssertFound.ThrowNotFoundIfNull(partition, command.PartitionGuid, EntityType.Partition);

        actor.ThrowIfAccessDeniedToPartition(partition);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.UpdateCompleted(partition.Guid, currentActor.ActorId);
    }
}
