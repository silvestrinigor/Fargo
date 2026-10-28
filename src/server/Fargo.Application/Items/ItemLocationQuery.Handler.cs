using Fargo.Application.Common;
using Fargo.Core.Actors;

namespace Fargo.Application.Items;

public sealed class ItemLocationQueryHandler(
    ActorResolver actorService,
    IItemQueryRepository itemRepository,
    ICurrentActor currentActor
) : IQueryHandler<ItemLocationQuery, IReadOnlyCollection<ItemDto>>
{
    public async Task<IReadOnlyCollection<ItemDto>> HandleAsync(
        ItemLocationQuery query, CancellationToken cancellationToken = default)
    {
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

        return item;
    }
}
