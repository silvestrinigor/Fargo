using Fargo.Application.Common;
using Fargo.Core.Actors;

namespace Fargo.Application.Articles;

/// <summary>
/// Handles queries that retrieve a paginated collection of articles accessible
/// to the current actor.
/// </summary>
/// <param name="actorService">Resolves the current actor and its partition access.</param>
/// <param name="articleRepository">Provides access to article query data.</param>
/// <param name="currentActor">Provides information about the currently authenticated actor.</param>
public sealed class ArticlesQueryHandler(
    ActorResolver actorService,
    IArticleQueryRepository articleRepository,
    ICurrentActor currentActor
) : IQueryHandler<ArticlesQuery, PagedResult<ArticleDto>>
{
    /// <summary>
    /// Handles the ArticlesQuery by retrieving a paginated collection of articles that are accessible to the current actor.
    /// </summary>
    /// <param name="query">The ArticlesQuery containing pagination and partition filtering parameters</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete</param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing a page of ArticleDto objects and the total count
    /// </returns>
    public async Task<PagedResult<ArticleDto>> HandleAsync(ArticlesQuery query, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        var partitionGuids =
            PartitionQueryFilter.ForPartitionedEntities(
                actor.PartitionAccessGuids,
                query.ChildOfAnyOfThesePartitions
            );

        var articles = await articleRepository.GetManyInfoOrderedByGuidAsync(
            query.WithPagination,
            partitionGuids,
            query.Sort,
            query.Search,
            cancellationToken
        );

        return articles;
    }
}
