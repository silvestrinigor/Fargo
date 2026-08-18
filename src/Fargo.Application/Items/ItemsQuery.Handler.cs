using Fargo.Application.Common;
using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Items;

public sealed class ItemsQueryHandler(
    ActorResolver actorService,
    IItemQueryRepository itemRepository,
    ICurrentActor currentActor,
    ILogger<ItemsQueryHandler> logger
) : IQueryHandler<ItemsQuery, IReadOnlyCollection<ItemDto>>
{
    public async Task<IReadOnlyCollection<ItemDto>> HandleAsync(
        ItemsQuery query,
        CancellationToken cancellationToken = default)
    {
        var pagination = query.WithPagination;

        logger.ManyQueryStarted(currentActor.Guid, currentActor.ActorType, query.WithPagination.Page, query.WithPagination.Limit);

        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        var partitionGuids =
            PartitionQueryFilter.ForPartitionedEntities(
                actor.PartitionAccessGuids,
                query.ChildOfAnyOfThesePartitions
            );

        var items = await itemRepository.GetManyInfoOrderedByGuidAsync(
            pagination,
            partitionGuids,
            cancellationToken
        );

        logger.ManyQueryCompleted(
            currentActor.Guid, currentActor.ActorType,
            query.ChildOfAnyOfThesePartitions?.Count ?? 0,
            partitionGuids?.Count ?? 0, items.Count
        );

        return items;
    }
}
