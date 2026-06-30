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
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Partition delete flow started for partition {partitionGuid} by actor {actorId}.",
                command.PartitionGuid, currentActor.ActorId);
        }

        var actor = await actorService.GetActorByActorIdAsync(currentActor.ActorId, cancellationToken);

        ActorAssertFound.ThrowNotAuthorizedIfNull(actor);

        actor.ThrowIfPermissionNotAuthorized(ActionType.DeletePartition);

        var partition = await partitionRepository.GetByGuid(command.PartitionGuid, cancellationToken);

        EntityAssertFound.ThrowNotFoundIfNull(partition);

        partitionRepository.Remove(partition);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Partition delete mutation completed for partition {partitionGuid} by actor {actorId}.",
                partition.Guid, actor.ActorId);
        }
    }
}
