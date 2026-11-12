using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Dtos.Partitions;
using Fargo.Application.Persistence.Repositories;
using Fargo.Core.Entities.Actors;

namespace Fargo.Application.Queries.Partitions;

public sealed record PartitionSingleQuery(
    Guid PartitionGuid
) : IQuery<PartitionDto?>;

public sealed class PartitionSingleQueryHandler(
    ActorResolver actorService,
    IPartitionQueryRepository partitionRepository,
    ICurrentActor currentActor
) : IQueryHandler<PartitionSingleQuery, PartitionDto?>
{
    public async Task<PartitionDto?> HandleAsync(
        PartitionSingleQuery query, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        var partition = await partitionRepository.GetInfoByGuid(
            query.PartitionGuid,
            childOfAnyOfThesePartitions: actor.PartitionAccessGuids,
            notChildOfAnyPartition: true, cancellationToken);

        return partition;
    }
}
