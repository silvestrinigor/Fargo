using Fargo.Application.Common;
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
        using var scope = logger.BeginActorScope(currentActor);
        logger.QueryStarted("Item container inventory query", "container", query.ItemContainerGuid);

        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        var itemExist = await itemQueryRepository.ExistByGuidAsync(query.ItemContainerGuid, actor.PartitionAccessGuids, cancellationToken);

        if (itemExist is false)
        {
            return null;
        }

        IReadOnlyCollection<ItemContainerInventoryDto>? inventory;

        if (query.At is { } at)
        {
            // Point-in-time inventory is resolved entirely from the containment
            // history in a single query, including the descendant walk.
            inventory = await itemQueryRepository.GetInventoryInfoByGuidAtInstantAsync(
                query.ItemContainerGuid,
                at,
                query.ArticleGuids,
                query.IncludeDescendents,
                cancellationToken);
        }
        else if (query.IncludeDescendents)
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

        // Event-tracked articles have no item rows, so their stock in the
        // container is computed separately from the movement-event stream and
        // merged with the entity-tracked counts. The event query honours the same
        // container, descendant, article-filter and point-in-time options.
        var eventInventory = await itemQueryRepository.GetEventInventoryInfoByGuidAsync(
            query.ItemContainerGuid,
            query.ArticleGuids,
            query.IncludeDescendents,
            query.At,
            cancellationToken);

        inventory = MergeByArticle(inventory, eventInventory);

        logger.QueryCompleted(
            "Item container inventory query", "container", query.ItemContainerGuid, found: inventory is not null);

        return inventory;
    }

    // An article is tracked either as entity or as event, so the two result sets
    // normally have disjoint article keys; the merge sums defensively by article
    // in case both ever contribute, and drops any entry that nets to zero.
    private static IReadOnlyCollection<ItemContainerInventoryDto> MergeByArticle(
        IReadOnlyCollection<ItemContainerInventoryDto> entityInventory,
        IReadOnlyCollection<ItemContainerInventoryDto> eventInventory)
    {
        return
        [
            .. entityInventory
                .Concat(eventInventory)
                .GroupBy(dto => dto.ArticleGuid)
                .Select(group => new ItemContainerInventoryDto(group.Key, group.Sum(dto => dto.TotalCount)))
                .Where(dto => dto.TotalCount != 0)
        ];
    }
}
