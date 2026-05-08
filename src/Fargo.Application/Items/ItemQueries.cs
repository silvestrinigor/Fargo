using Fargo.Application.Authentication;
using Fargo.Domain;

namespace Fargo.Application.Items;

#region Single

public sealed record ItemSingleQuery(
    Guid ItemGuid,
    DateTimeOffset? AsOfDateTime = null
) : IQuery<ItemDto?>;

public sealed class ItemSingleQueryHandler(
    ActorService actorService,
    IItemQueryRepository itemRepository,
    ICurrentUser currentUser
) : IQueryHandler<ItemSingleQuery, ItemDto?>
{
    public async Task<ItemDto?> Handle(
        ItemSingleQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        var item = await itemRepository.GetInfoByGuid(
            query.ItemGuid,
            query.AsOfDateTime,
            actor.PartitionAccessesGuids,
            notInsideAnyPartition: true,
            cancellationToken
        );

        return item;
    }
}

#endregion Single

#region Many

public sealed record ItemsQuery(
    Pagination WithPagination,
    DateTimeOffset? TemporalAsOfDateTime = null,
    IReadOnlyCollection<Guid>? InsideAnyOfThisPartitions = null,
    bool? NotInsideAnyPartition = null
) : IQuery<IReadOnlyCollection<ItemDto>>;

public sealed class ItemsQueryHandler(
    ActorService actorService,
    IItemQueryRepository itemRepository,
    ICurrentUser currentUser
) : IQueryHandler<ItemsQuery, IReadOnlyCollection<ItemDto>>
{
    public async Task<IReadOnlyCollection<ItemDto>> Handle(
        ItemsQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        var insideAnyOfThisPartitions = query.InsideAnyOfThisPartitions is { } requested
            ? [.. actor.PartitionAccessesGuids.Intersect(requested)]
            : query.NotInsideAnyPartition is true
                ? null
                : actor.PartitionAccessesGuids;
        var notInsideAnyPartition = query.NotInsideAnyPartition ??
            (query.InsideAnyOfThisPartitions is null ? true : null);

        var items = await itemRepository.GetManyInfo(
            query.WithPagination,
            query.TemporalAsOfDateTime,
            insideAnyOfThisPartitions,
            notInsideAnyPartition,
            cancellationToken
        );

        return items;
    }
}

#endregion Many
