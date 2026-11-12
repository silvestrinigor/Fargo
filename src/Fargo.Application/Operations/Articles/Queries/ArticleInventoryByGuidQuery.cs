using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Operations.Articles.Dtos;
using Fargo.Application.Persistence.Repositories;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Articles;

namespace Fargo.Application.Operations.Articles;

/// <summary>
/// Represents a query for retrieving article inventory information by article GUID,
/// optionally as it existed at a specific point in time.
/// This query can optionally include specific item container GUIDs and control whether descendant items are included in the results.
/// </summary>
/// <param name="ArticleGuid">The unique identifier of the article to retrieve inventory for</param>
/// <param name="InsideItemContainerGuids">Optional collection of item container GUIDs to filter the inventory by</param>
/// <param name="IncludeDescendents">Flag indicating whether descendant items should be included in the results (defaults to true)</param>
/// <param name="At">
/// The instant, in UTC, at which the inventory is evaluated. When
/// <see langword="null"/>, the current inventory is returned. When provided, the
/// inventory is reconstructed from the item containment history as it was at that
/// instant.
/// </param>
public sealed record ArticleInventoryByGuidQuery(
    Guid ArticleGuid,
    IReadOnlyCollection<Guid>? InsideItemContainerGuids = null,
    bool IncludeDescendents = true,
    DateTimeOffset? At = null
) : IQuery<ArticleInventoryDto?>;

/// <summary>
/// Handles queries to retrieve inventory information for a specific article.
/// </summary>
public sealed class ArticleInventoryQueryHandler(
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
