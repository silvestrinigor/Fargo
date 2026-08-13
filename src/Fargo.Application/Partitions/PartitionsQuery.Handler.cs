using Fargo.Application.Common;
using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Partitions;

public sealed class PartitionsQueryHandler(
    ActorResolver actorService,
    IPartitionQueryRepository partitionRepository,
    ICurrentActor currentActor,
    ILogger<PartitionsQueryHandler> logger
) : IQueryHandler<PartitionsQuery, IReadOnlyCollection<PartitionDto>>
{
    public async Task<IReadOnlyCollection<PartitionDto>> HandleAsync(
        PartitionsQuery query, CancellationToken cancellationToken = default)
    {
        logger.ManyQueryStarted(currentActor.Guid, query.WithPagination.Page, query.WithPagination.Limit);

        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        var partitionGuids =
            PartitionQueryFilter.ForPartitionedEntities(
                actor.PartitionAccessGuids,
                query.ChildOfAnyOfThesePartitions);

        var partitions = await partitionRepository.GetManyInfo(
            query.WithPagination, partitionGuids,
            cancellationToken);

        logger.ManyQueryCompleted(
            currentActor.Guid,
            query.ChildOfAnyOfThesePartitions?.Count ?? 0,
            partitionGuids?.Count ?? 0, partitions.Count);

        return partitions;
    }
}
