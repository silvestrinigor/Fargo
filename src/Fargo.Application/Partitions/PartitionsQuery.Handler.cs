using Fargo.Application.Identity;
using Fargo.Application.Shared.Partitions;
using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Partitions;

public sealed class PartitionsQueryHandler(
    ActorService actorService,
    IPartitionQueryRepository partitionRepository,
    ICurrentActor currentActor,
    ILogger<PartitionsQueryHandler> logger
) : IQueryHandler<PartitionsQuery, IReadOnlyCollection<PartitionDto>>
{
    public async Task<IReadOnlyCollection<PartitionDto>> HandleAsync(
        PartitionsQuery query, CancellationToken cancellationToken = default)
    {
        logger.ManyQueryStarted(currentActor.ActorId, query.WithPagination.Page, query.WithPagination.Limit);

        var actor = await actorService.GetActorByActorIdAsync(currentActor.ActorId, cancellationToken);

        ActorNotFoundFargoException.ThrowIfNull(actor, currentActor.ActorId);

        var (childOfAnyOfThesePartitions, notChildOfAnyPartition) =
            PartitionQueryFilter.ForPartitionedEntities(
                actor.PartitionAccessGuids,
                query.ChildOfAnyOfThesePartitions,
                query.NotChildOfAnyPartition);

        var partitions = await partitionRepository.GetManyInfo(
            query.WithPagination, query.TemporalAsOfDateTime, childOfAnyOfThesePartitions,
            notChildOfAnyPartition, cancellationToken);

        logger.ManyQueryCompleted(
            currentActor.ActorId,
            query.ChildOfAnyOfThesePartitions?.Count ?? 0,
            childOfAnyOfThesePartitions?.Count ?? 0, partitions.Count);

        return partitions;
    }
}
