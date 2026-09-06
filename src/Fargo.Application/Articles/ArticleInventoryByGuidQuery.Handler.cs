using Fargo.Application.Common;
using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Fargo.Core.Items;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles;

/// <summary>
/// Handles queries to retrieve inventory information for a specific article identified by its GUID.
/// </summary>
public sealed class ArticleInventoryByGuidQueryHandler(
    ActorResolver actorService,
    IArticleQueryRepository articleRepository,
    IItemRepository itemRepository,
    ICurrentActor currentActor,
    ILogger<ArticleInventoryByGuidQueryHandler> logger
) : IQueryHandler<ArticleInventoryByGuidQuery, ArticleInventoryDto?>
{
    /// <summary>
    /// Processes the inventory lookup for an article.
    /// </summary>
    /// <param name="query">The query containing the Article GUID and optional container filters.</param>
    /// <param name="cancellationToken">The cancellation token for the asynchronous operation.</param>
    /// <returns>A task that returns the <see cref="ArticleInventoryDto"/> if found; otherwise, null.</returns>
    /// <remarks>
    /// If <see cref="ArticleInventoryByGuidQuery.IncludeDescendents"/> is true and container IDs are provided, 
    /// the handler will recursively resolve all descendant container IDs before fetching inventory data.
    /// </remarks>
    public async Task<ArticleInventoryDto?> HandleAsync(
        ArticleInventoryByGuidQuery query, CancellationToken cancellationToken = default)
    {
        logger.ArticleInventoryQueryByGuidStarted(query.ArticleGuid, currentActor.Guid, currentActor.ActorType);

        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        var articleExist = await articleRepository.ExistByGuidAsync(query.ArticleGuid, actor.PartitionAccessGuids, cancellationToken);

        ArticleInventoryDto? inventory = null;

        if (articleExist && query.IncludeDescendents && query.InsideItemContainerGuids is { Count: > 0 })
        {
            var itemContainerGuids = new List<Guid>();

            foreach (var containerGuid in query.InsideItemContainerGuids)
            {
                // Fetch all nested/sub-container IDs.
                var result = await itemRepository.GetContainedDescendantGuidsAsync(containerGuid, true, cancellationToken);
                itemContainerGuids.AddRange(result);
            }

            inventory = await articleRepository.GetInventoryInfoByGuidAsync(
                query.ArticleGuid,
                [.. itemContainerGuids.Distinct()],
                cancellationToken);
        }
        else if (articleExist)
        {
            // Standard query: Use only the provided container IDs without recursion.
            inventory = await articleRepository.GetInventoryInfoByGuidAsync(
                query.ArticleGuid,
                query.InsideItemContainerGuids,
                cancellationToken);
        }

        logger.ArticleInventoryQueryByGuidCompleted(
            query.ArticleGuid,
            currentActor.Guid,
            currentActor.ActorType,
            inventory is not null,
            inventory?.TotalCount ?? 0
        );

        return inventory;
    }
}
