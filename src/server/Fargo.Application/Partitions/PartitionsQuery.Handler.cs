using Fargo.Application.Common;
using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Partitions;

public sealed class PartitionsQueryHandler(
    ActorResolver actorService,
    IPartitionQueryRepository partitionRepository,
    ICurrentActor currentActor,
    ILogger<PartitionsQueryHandler> logger
) : IQueryHandler<PartitionsQuery, PagedResult<PartitionDto>>
{
    public async Task<PagedResult<PartitionDto>> HandleAsync(
        PartitionsQuery query, CancellationToken cancellationToken = default)
    {
        using var scope = logger.BeginActorScope(currentActor);
        logger.CollectionStarted("Partitions query", query.WithPagination.Page.Value, query.WithPagination.Limit.Value);

        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        var partitionGuids =
            PartitionQueryFilter.ForPartitionedEntities(
                actor.PartitionAccessGuids,
                query.ChildOfAnyOfThesePartitions);

        var partitions = await partitionRepository.GetManyInfo(
            query.WithPagination, partitionGuids,
            cancellationToken);

        logger.CollectionCompleted("Partitions query", partitions.Items.Count);

        return partitions;
    }
}
