using Fargo.Application.Common;
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

        logger.ManyQueryStarted(currentActor.Guid, query.WithPagination.Page, query.WithPagination.Limit);

        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        var (childOfAnyOfThesePartitions, notChildOfAnyPartition) =
            PartitionQueryFilter.ForPartitionedEntities(
                actor.PartitionAccessGuids,
                query.ChildOfAnyOfThesePartitions,
                query.NotChildOfAnyPartition);

        var items = await itemRepository.GetManyInfo(
            pagination,
            childOfAnyOfThesePartitions, notChildOfAnyPartition,
            cancellationToken);

        logger.ManyQueryCompleted(
            currentActor.Guid, query.ChildOfAnyOfThesePartitions?.Count ?? 0,
            childOfAnyOfThesePartitions?.Count ?? 0, items.Count);

        return items;
    }
}
