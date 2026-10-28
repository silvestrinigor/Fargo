using Fargo.Application.Common;
using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Partitions;

public sealed class PartitionSingleQueryHandler(
    ActorResolver actorService,
    IPartitionQueryRepository partitionRepository,
    ICurrentActor currentActor,
    ILogger<PartitionSingleQueryHandler> logger
) : IQueryHandler<PartitionSingleQuery, PartitionDto?>
{
    public async Task<PartitionDto?> HandleAsync(
        PartitionSingleQuery query, CancellationToken cancellationToken = default)
    {
        using var scope = logger.BeginActorScope(currentActor);
        logger.QueryStarted("Partition single query", "partition", query.PartitionGuid);

        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        var partition = await partitionRepository.GetInfoByGuid(
            query.PartitionGuid,
            childOfAnyOfThesePartitions: actor.PartitionAccessGuids,
            notChildOfAnyPartition: true, cancellationToken);

        logger.QueryCompleted("Partition single query", "partition", query.PartitionGuid, found: partition is not null);

        return partition;
    }
}
