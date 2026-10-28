using Fargo.Application.Common;
using Fargo.Core.Actors;

namespace Fargo.Application.Partitions;

public sealed class PartitionsQueryHandler(
    ActorResolver actorService,
    IPartitionQueryRepository partitionRepository,
    ICurrentActor currentActor
) : IQueryHandler<PartitionsQuery, PagedResult<PartitionDto>>
{
    public async Task<PagedResult<PartitionDto>> HandleAsync(
        PartitionsQuery query, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        var partitionGuids =
            PartitionQueryFilter.ForPartitionedEntities(
                actor.PartitionAccessGuids,
                query.ChildOfAnyOfThesePartitions);

        var partitions = await partitionRepository.GetManyInfo(
            query.WithPagination, partitionGuids,
            cancellationToken);

        return partitions;
    }
}
