using Fargo.Application.Abstractions;
using Fargo.Application.Common;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Paging;
using Fargo.Application.Operations.Items.Dtos;
using Fargo.Application.Persistence.Repositories;
using Fargo.Core.Entities.Actors;


namespace Fargo.Application.Operations.Items;

public sealed record ItemsQuery(
    Pagination WithPagination,
    IReadOnlyCollection<Guid>? ChildOfAnyOfThesePartitions = null,
    Sort? Sort = null,
    string? Search = null
) : IQuery<PagedResult<ItemDto>>;

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
