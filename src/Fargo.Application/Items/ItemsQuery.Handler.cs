using Fargo.Application.Identity;
using Fargo.Application.Shared.Items;
using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Items;

public sealed class ItemsQueryHandler(
    ActorService actorService,
    IItemQueryRepository itemRepository,
    ICurrentActor currentActor,
    ILogger<ItemsQueryHandler> logger
) : IQueryHandler<ItemsQuery, IReadOnlyCollection<ItemDto>>
{
    public async Task<IReadOnlyCollection<ItemDto>> HandleAsync(
        ItemsQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var pagination = query.WithPagination;

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Items query started for actor {actorId}. Page: {page}. Limit: {limit}.",
                currentActor.ActorId, pagination.Page, pagination.Limit);
        }

        var actor = await actorService.GetActorByActorIdAsync(currentActor.ActorId, cancellationToken);

        ActorAssertFound.ThrowNotAuthorizedIfNull(actor);

        var (childOfAnyOfThesePartitions, notChildOfAnyPartition) =
            PartitionQueryFilter.ForPartitionedEntities(
                actor.PartitionAccessGuids,
                query.ChildOfAnyOfThesePartitions,
                query.NotChildOfAnyPartition);

        var items = await itemRepository.GetManyInfo(
            pagination,
            query.TemporalAsOfDateTime,
            childOfAnyOfThesePartitions,
            notChildOfAnyPartition,
            cancellationToken);

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Items query completed for actor {actorId}. RequestedPartitionCount: {requestedPartitionCount}. EffectivePartitionCount: {effectivePartitionCount}. ResultCount: {resultCount}.",
                actor.ActorId, query.ChildOfAnyOfThesePartitions?.Count ?? 0,
                childOfAnyOfThesePartitions?.Count ?? 0, items.Count);
        }

        return items;
    }
}
