using Fargo.Application.Common;
using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Items;

public sealed class ItemLocationQueryHandler(
    ActorResolver actorService,
    IItemQueryRepository itemRepository,
    ICurrentActor currentActor,
    ILogger<ItemLocationQueryHandler> logger
) : IQueryHandler<ItemLocationQuery, IReadOnlyCollection<ItemDto>>
{
    public async Task<IReadOnlyCollection<ItemDto>> HandleAsync(
        ItemLocationQuery query, CancellationToken cancellationToken = default)
    {
        using var scope = logger.BeginActorScope(currentActor);
        logger.QueryStarted("Item location query", "item", query.ItemGuid);

        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        var item = query.At is { } at
            ? await itemRepository.GetLocationInfoByGuidAtInstantOrderedByDepthAsync(
                query.ItemGuid,
                at,
                actor.PartitionAccessGuids,
                cancellationToken)
            : await itemRepository.GetLocationInfoByGuidOrderedByDepthAsync(
                query.ItemGuid,
                actor.PartitionAccessGuids,
                cancellationToken);

        logger.QueryCompleted("Item location query", "item", query.ItemGuid, found: item.Count != 0);

        return item;
    }
}
