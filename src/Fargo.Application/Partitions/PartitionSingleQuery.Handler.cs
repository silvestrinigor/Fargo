using Fargo.Application.Identity;
using Fargo.Application.Shared.Partitions;
using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Partitions;

public sealed class PartitionSingleQueryHandler(
    ActorService actorService,
    IPartitionQueryRepository partitionRepository,
    ICurrentActor currentActor,
    ILogger<PartitionSingleQueryHandler> logger
) : IQueryHandler<PartitionSingleQuery, PartitionDto?>
{
    public async Task<PartitionDto?> HandleAsync(
        PartitionSingleQuery query, CancellationToken cancellationToken = default)
    {
        logger.SingleQueryStarted(query.PartitionGuid, currentActor.ActorId);

        var actor = await actorService.GetActorByActorIdAsync(currentActor.ActorId, cancellationToken);

        ActorAssertFound.ThrowNotAuthorizedIfNull(actor);

        var partition = await partitionRepository.GetInfoByGuid(
            query.PartitionGuid, query.AsOfDateTime,
            childOfAnyOfThesePartitions: actor.PartitionAccessGuids,
            notChildOfAnyPartition: true, cancellationToken);

        logger.SingleQueryCompleted(query.PartitionGuid, currentActor.ActorId, partition is not null);

        return partition;
    }
}
