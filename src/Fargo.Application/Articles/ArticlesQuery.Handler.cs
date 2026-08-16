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
    public async Task<IReadOnlyCollection<ArticleDto>> HandleAsync(
        ArticlesQuery query,
        CancellationToken cancellationToken = default)
    {
        logger.ArticlesQueryStarted(currentActor.Guid, currentActor.ActorType, query.WithPagination.Page, query.WithPagination.Limit);

        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        var partitionGuids =
            PartitionQueryFilter.ForPartitionedEntities(
                actor.PartitionAccessGuids,
                query.ChildOfAnyOfThesePartitions);

        var articles = await articleRepository.GetManyInfoAsync(
            query.WithPagination,
            partitionGuids,
            cancellationToken);

        logger.ArticlesQueryCompleted(
            actor.Guid,
            query.ChildOfAnyOfThesePartitions?.Count ?? 0,
            partitionGuids?.Count ?? 0,
            articles.Count);

        return articles;
    }
}
