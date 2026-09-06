using Fargo.Application.Common;
using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles;

/// <summary>
/// Handles queries that retrieve a paginated collection of articles accessible
/// to the current actor.
/// </summary>
/// <param name="actorService">Resolves the current actor and its partition access.</param>
/// <param name="articleRepository">Provides access to article query data.</param>
/// <param name="currentActor">Provides information about the currently authenticated actor.</param>
/// <param name="logger">Logs the execution of the query.</param>
public sealed class ArticlesQueryHandler(
    ActorResolver actorService,
    IArticleQueryRepository articleRepository,
    ICurrentActor currentActor,
    ILogger<ArticlesQueryHandler> logger
) : IQueryHandler<ArticlesQuery, IReadOnlyCollection<ArticleDto>>
{
    /// <summary>
    /// Handles the ArticlesQuery by retrieving a paginated collection of articles that are accessible to the current actor.
    /// </summary>
    /// <param name="query">The ArticlesQuery containing pagination and partition filtering parameters</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete</param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing a read-only collection of ArticleDto objects
    /// </returns>
    public async Task<IReadOnlyCollection<ArticleDto>> HandleAsync(ArticlesQuery query, CancellationToken cancellationToken = default)
    {
        logger.ArticlesQueryStarted(currentActor.Guid, currentActor.ActorType, query.WithPagination.Page, query.WithPagination.Limit);

        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        var partitionGuids =
            PartitionQueryFilter.ForPartitionedEntities(
                actor.PartitionAccessGuids,
                query.ChildOfAnyOfThesePartitions
            );

        var articles = await articleRepository.GetManyInfoOrderedByGuidAsync(
            query.WithPagination,
            partitionGuids,
            cancellationToken
        );

        logger.ArticlesQueryCompleted(
            actor.Guid,
            actor.ActorType,
            requestedPartitionCount: query.ChildOfAnyOfThesePartitions?.Count ?? 0,
            effectivePartitionCount: partitionGuids?.Count ?? 0,
            resultCount: articles.Count
        );

        return articles;
    }
}
