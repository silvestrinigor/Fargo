using Fargo.Application.Common;
using Fargo.Application.Identity;
using Fargo.Application.Shared.Items;
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
        logger.SingleQueryStarted(query.ItemGuid, currentActor.Guid);

        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        var item = await itemRepository.GetInfoByGuid(
            query.ItemGuid,
            actor.PartitionAccessGuids, notChildOfAnyPartition: true,
            cancellationToken);

        logger.SingleQueryCompleted(query.ItemGuid, actor.Guid, item is not null);

        return item;
    }
}
