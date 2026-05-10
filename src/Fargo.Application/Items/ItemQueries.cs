using Fargo.Application.Authentication;
using Fargo.Domain;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Items;

#region Single

public sealed record ItemSingleQuery(
    Guid ItemGuid,
    DateTimeOffset? AsOfDateTime = null
) : IQuery<ItemDto?>;

public sealed class ItemSingleQueryHandler(
    ActorService actorService,
    IItemQueryRepository itemRepository,
    ICurrentUser currentUser,
    ILogger<ItemSingleQueryHandler> logger
) : IQueryHandler<ItemSingleQuery, ItemDto?>
{
    public async Task<ItemDto?> Handle(
        ItemSingleQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var actorGuid = currentUser.UserGuid;

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Item single query started for item {ItemGuid} by actor {ActorGuid}.",
                query.ItemGuid,
                actorGuid);
        }

        var actor = await actorService.GetAuthorizedActorByGuid(actorGuid, cancellationToken);

        var item = await itemRepository.GetInfoByGuid(
            query.ItemGuid,
            query.AsOfDateTime,
            actor.PartitionAccessesGuids,
            notInsideAnyPartition: true,
            cancellationToken
        );

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Item single query completed for item {ItemGuid} by actor {ActorGuid}. Found: {Found}.",
                query.ItemGuid,
                actor.Guid,
                item is not null);
        }

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
    ICurrentUser currentUser,
    ILogger<ItemsQueryHandler> logger
) : IQueryHandler<ItemsQuery, IReadOnlyCollection<ItemDto>>
{
    public async Task<IReadOnlyCollection<ItemDto>> Handle(
        ItemsQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var actorGuid = currentUser.UserGuid;
        var pagination = query.WithPagination;

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Items query started for actor {ActorGuid}. Page: {Page}. Limit: {Limit}.",
                actorGuid,
                pagination.Page,
                pagination.Limit);
        }

        var actor = await actorService.GetAuthorizedActorByGuid(actorGuid, cancellationToken);

        var insideAnyOfThisPartitions = query.InsideAnyOfThisPartitions is { } requested
            ? [.. actor.PartitionAccessesGuids.Intersect(requested)]
            : actor.PartitionAccessesGuids;

        var items = await itemRepository.GetManyInfo(
            pagination,
            query.TemporalAsOfDateTime,
            insideAnyOfThisPartitions,
            query.NotInsideAnyPartition,
            cancellationToken
        );

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Items query completed for actor {ActorGuid}. RequestedPartitionCount: {RequestedPartitionCount}. EffectivePartitionCount: {EffectivePartitionCount}. ResultCount: {ResultCount}.",
                actor.Guid,
                query.InsideAnyOfThisPartitions?.Count ?? 0,
                insideAnyOfThisPartitions?.Count ?? 0,
                items.Count);
        }

        return items;
    }
}

#endregion Many
