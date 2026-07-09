using Fargo.Application.Identity;
using Fargo.Application.Shared.Items;
using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Items;

public sealed class ItemSingleQueryHandler(
    ActorService actorService, IItemQueryRepository itemRepository,
    ICurrentActor currentActor, ILogger<ItemSingleQueryHandler> logger
) : IQueryHandler<ItemSingleQuery, ItemDto?>
{
    public async Task<ItemDto?> HandleAsync(
        ItemSingleQuery query, CancellationToken cancellationToken = default)
    {
        logger.SingleQueryStarted(query.ItemGuid, currentActor.ActorId);

        var actor = await actorService.GetActorByActorIdAsync(currentActor.ActorId, cancellationToken);

        ActorAssertFound.ThrowNotAuthorizedIfNull(actor);

        var item = await itemRepository.GetInfoByGuid(
            query.ItemGuid, query.AsOfDateTime,
            actor.PartitionAccessGuids, notChildOfAnyPartition: true,
            cancellationToken);

        logger.SingleQueryCompleted(query.ItemGuid, actor.ActorId, item is not null);

        return item;
    }
}
