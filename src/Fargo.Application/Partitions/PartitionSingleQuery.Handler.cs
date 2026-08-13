using Fargo.Application.Common;
using Fargo.Application.Identity;
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
        logger.SingleQueryStarted(query.PartitionGuid, currentActor.Guid);

        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        var partition = await partitionRepository.GetInfoByGuid(
            query.PartitionGuid,
            childOfAnyOfThesePartitions: actor.PartitionAccessGuids,
            notChildOfAnyPartition: true, cancellationToken);

        logger.SingleQueryCompleted(query.PartitionGuid, currentActor.Guid, partition is not null);

        return partition;
    }
}
