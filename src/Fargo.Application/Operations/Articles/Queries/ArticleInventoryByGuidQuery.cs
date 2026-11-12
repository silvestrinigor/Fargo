using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Operations.Articles.Dtos;
using Fargo.Application.Persistence.Repositories;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Articles;

namespace Fargo.Application.Operations.Articles;

/// <param name="At">
/// UTC instant to evaluate the inventory at, rebuilt from the containment history.
/// <see langword="null"/> returns the current inventory.
/// </param>
public sealed record ArticleInventoryByGuidQuery(
    Guid ArticleGuid,
    IReadOnlyCollection<Guid>? InsideItemContainerGuids = null,
    bool IncludeDescendents = true,
    DateTimeOffset? At = null
) : IQuery<ArticleInventoryDto?>;

public sealed class ArticleInventoryQueryHandler(
    ActorResolver actorService,
    IArticleQueryRepository articleRepository,
    ICurrentActor currentActor
) : IQueryHandler<ArticleInventoryByGuidQuery, ArticleInventoryDto?>
{
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
