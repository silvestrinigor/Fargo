using Fargo.Application.Common;
using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Fargo.Core.Items;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Items;

public sealed class ItemContainerInventoryByGuidQueryHandler(
    ActorResolver actorService,
    IItemQueryRepository itemQueryRepository,
    IItemRepository itemRepository,
    ICurrentActor currentActor,
    ILogger<ItemContainerInventoryByGuidQueryHandler> logger
) : IQueryHandler<ItemContainerInventoryByGuidQuery, IReadOnlyCollection<ItemContainerInventoryDto>?>
{
    public async Task<IReadOnlyCollection<ItemContainerInventoryDto>?> HandleAsync(ItemContainerInventoryByGuidQuery query, CancellationToken cancellationToken = default)
    {
        logger.QueryInventoryByGuidStarted(query.ItemContainerGuid, currentActor.Guid, currentActor.ActorType);

        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        var itemExist = await itemQueryRepository.ExistByGuidAsync(query.ItemContainerGuid, actor.PartitionAccessGuids, cancellationToken);

        if (itemExist is false)
        {
            return null;
        }

        IReadOnlyCollection<ItemContainerInventoryDto>? inventory;

        if (query.IncludeDescendents)
        {
            var itemContainerGuids = await itemRepository.GetContainedDescendantGuidsAsync(query.ItemContainerGuid, true, cancellationToken);

            inventory = await itemQueryRepository.GetInventoryInfoByGuidAsync(
                itemContainerGuids, query.ArticleGuids, cancellationToken);
        }
        else
        {
            inventory = await itemQueryRepository.GetInventoryInfoByGuidAsync(
                [query.ItemContainerGuid], query.ArticleGuids, cancellationToken);
        }

        logger.QueryInventoryByGuidCompleted(
            query.ItemContainerGuid, currentActor.Guid, currentActor.ActorType, inventory is not null);

        return inventory;
    }
}
