using Fargo.Application.Common;
using Fargo.Application.Identity;
using Fargo.Application.Shared.Articles;
using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles;

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
        logger.ArticlesQueryStarted(currentActor.Guid, query.WithPagination.Page, query.WithPagination.Limit);

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
