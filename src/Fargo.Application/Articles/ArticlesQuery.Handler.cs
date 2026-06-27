using Fargo.Application.Identity;
using Fargo.Application.Shared.Articles;
using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles;

public sealed class ArticlesQueryHandler(
    ActorService actorService,
    IArticleQueryRepository articleRepository,
    ICurrentActor currentActor,
    ILogger<ArticlesQueryHandler> logger
) : IQueryHandler<ArticlesQuery, IReadOnlyCollection<ArticleDto>>
{
    public async Task<IReadOnlyCollection<ArticleDto>> HandleAsync(
        ArticlesQuery query,
        CancellationToken cancellationToken = default)
    {
        var pagination = query.WithPagination;

        logger.ArticlesQueryStarted(currentActor.ActorId, pagination.Page, pagination.Limit);

        var actor = await actorService.GetActorByActorIdAsync(currentActor.ActorId, cancellationToken);

        ActorAssertFound.ThrowNotAuthorizedIfNull(actor);

        var (childOfAnyOfThesePartitions, notChildOfAnyPartition) =
            PartitionQueryFilter.ForPartitionedEntities(
                actor.PartitionAccessGuids,
                query.ChildOfAnyOfThesePartitions,
                query.NotChildOfAnyPartition);

        var articles = await articleRepository.GetManyInfo(
            pagination,
            query.TemporalAsOfDateTime,
            childOfAnyOfThesePartitions,
            notChildOfAnyPartition,
            cancellationToken
        );

        logger.ArticlesQueryCompleted(actor.ActorId, query.ChildOfAnyOfThesePartitions?.Count ?? 0, childOfAnyOfThesePartitions?.Count ?? 0, articles.Count);

        return articles;
    }
}
