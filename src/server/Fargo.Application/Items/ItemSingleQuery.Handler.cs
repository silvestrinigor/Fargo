using Fargo.Application.Common;
using Fargo.Core.Actors;

namespace Fargo.Application.Items;

public sealed class ItemSingleQueryHandler(
    ActorResolver actorService, IItemQueryRepository itemRepository,
    ICurrentActor currentActor
) : IQueryHandler<ItemSingleQuery, ItemDto?>
{
    public async Task<ItemDto?> HandleAsync(
        ItemSingleQuery query, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        var item = await itemRepository.GetInfoByGuidAsync(
            query.ItemGuid,
            actor.PartitionAccessGuids,
            cancellationToken
        );

        return item;
    }
}
