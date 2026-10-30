using Fargo.Application.Common;
using Fargo.Core.Actors;

namespace Fargo.Application.Items;

public sealed class ItemsQueryHandler(
    ActorResolver actorService,
    IItemQueryRepository itemRepository,
    ICurrentActor currentActor
) : IQueryHandler<ItemsQuery, PagedResult<ItemDto>>
{
    public async Task<PagedResult<ItemDto>> HandleAsync(
        ItemsQuery query,
        CancellationToken cancellationToken = default)
    {
        var pagination = query.WithPagination;

        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        var partitionGuids =
            PartitionQueryFilter.ForPartitionedEntities(
                actor.PartitionAccessGuids,
                query.ChildOfAnyOfThesePartitions
            );

        var items = await itemRepository.GetManyInfoOrderedByGuidAsync(
            pagination,
            partitionGuids,
            query.Sort,
            query.Search,
            cancellationToken
        );

        return items;
    }
}
