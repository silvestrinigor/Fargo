using Fargo.Application.Common;
using Fargo.Core.Actors;
using Fargo.Core.Articles;

namespace Fargo.Application.Articles;

/// <summary>
/// Handles queries to retrieve inventory information for a specific article identified by its GUID.
/// </summary>
public sealed class ArticleInventoryByGuidQueryHandler(
    ActorResolver actorService,
    IArticleQueryRepository articleRepository,
    ICurrentActor currentActor
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
    public async Task<ArticleInventoryDto?> HandleAsync(ArticleInventoryByGuidQuery query, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        // Resolving the tracking mode also serves as the existence/access check:
        // a null result means the article does not exist (or is not accessible)
        // within the actor's partitions.
        var trackingMode = await articleRepository.GetTrackingModeByGuidAsync(
            query.ArticleGuid, actor.PartitionAccessGuids, cancellationToken);

        ArticleInventoryDto? inventory = null;

        if (trackingMode is ArticleTrackingMode.Event)
        {
            // Event-tracked articles have no item rows; inventory is the net of
            // signed movement quantities derived from the event stream.
            inventory = await articleRepository.GetEventInventoryInfoByGuidAsync(
                query.ArticleGuid,
                query.InsideItemContainerGuids,
                query.IncludeDescendents,
                query.At,
                cancellationToken);
        }
        else if (trackingMode is ArticleTrackingMode.Entity && query.At is { } at)
        {
            // Point-in-time inventory is resolved entirely from the containment
            // history in a single query, including the descendant walk.
            inventory = await articleRepository.GetInventoryInfoByGuidAtInstantAsync(
                query.ArticleGuid,
                at,
                query.InsideItemContainerGuids,
                query.IncludeDescendents,
                cancellationToken);
        }
        else if (trackingMode is ArticleTrackingMode.Entity && query.IncludeDescendents && query.InsideItemContainerGuids is { Count: > 0 } containers)
        {
            // Resolve the containers and all their descendant containers in a
            // single recursive query rather than walking each container in the
            // application and round-tripping a large IN list.
            inventory = await articleRepository.GetInventoryInfoByGuidWithDescendantsAsync(
                query.ArticleGuid,
                containers,
                cancellationToken);
        }
        else if (trackingMode is ArticleTrackingMode.Entity)
        {
            // Standard query: Use only the provided container IDs without recursion.
            inventory = await articleRepository.GetInventoryInfoByGuidAsync(
                query.ArticleGuid,
                query.InsideItemContainerGuids,
                cancellationToken);
        }

        return inventory;
    }
}
