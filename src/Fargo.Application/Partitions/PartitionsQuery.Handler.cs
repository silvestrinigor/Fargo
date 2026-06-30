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
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Partitions query started for actor {actorId}. Page: {page}. Limit: {limit}.",
                currentActor.ActorId,
                query.WithPagination.Page,
                query.WithPagination.Limit);
        }

        var actor = await actorService.GetActorByActorIdAsync(currentActor.ActorId, cancellationToken);

        ActorAssertFound.ThrowNotAuthorizedIfNull(actor);

        var (childOfAnyOfThesePartitions, notChildOfAnyPartition) =
            PartitionQueryFilter.ForPartitionedEntities(
                actor.PartitionAccessGuids,
                query.ChildOfAnyOfThesePartitions,
                query.NotChildOfAnyPartition);

        var partitions = await partitionRepository.GetManyInfo(
            query.WithPagination, query.TemporalAsOfDateTime, childOfAnyOfThesePartitions,
            notChildOfAnyPartition, cancellationToken);

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Partitions query completed for actor {actorId}. RequestedPartitionCount: {requestedPartitionCount}. EffectivePartitionCount: {effectivePartitionCount}. ResultCount: {resultCount}.",
                actor.ActorId, query.ChildOfAnyOfThesePartitions?.Count ?? 0,
                childOfAnyOfThesePartitions?.Count ?? 0, partitions.Count);
        }

        return partitions;
    }
}
