using Fargo.Application.Common;
using Fargo.Application.Identity;
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
        logger.LocationQueryStarted(query.ItemGuid, currentActor.Guid);

        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        var item = await itemRepository.GetLocationInfoByGuidOrderedByDepthAsync(
            query.ItemGuid,
            actor.PartitionAccessGuids,
            cancellationToken);

        logger.LocationQueryCompleted(query.ItemGuid, currentActor.Guid, item.Count != 0);

        return item;
    }
}
