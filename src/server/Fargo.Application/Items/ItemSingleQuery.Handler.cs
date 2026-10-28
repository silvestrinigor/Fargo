using Fargo.Application.Common;
using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Items;

public sealed class ItemSingleQueryHandler(
    ActorResolver actorService, IItemQueryRepository itemRepository,
    ICurrentActor currentActor, ILogger<ItemSingleQueryHandler> logger
) : IQueryHandler<ItemSingleQuery, ItemDto?>
{
    public async Task<ItemDto?> HandleAsync(
        ItemSingleQuery query, CancellationToken cancellationToken = default)
    {
        using var scope = logger.BeginActorScope(currentActor);
        logger.QueryStarted("Item single query", "item", query.ItemGuid);

        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        var item = await itemRepository.GetInfoByGuidAsync(
            query.ItemGuid,
            actor.PartitionAccessGuids,
            cancellationToken
        );

        logger.QueryCompleted("Item single query", "item", query.ItemGuid, found: item is not null);

        return item;
    }
}
