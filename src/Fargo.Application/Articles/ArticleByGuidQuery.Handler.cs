using Fargo.Application.Identity;
using Fargo.Application.Shared.Articles;
using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles;

public sealed class ArticleByGuidQueryHandler(
    ActorService actorService,
    IArticleQueryRepository articleRepository,
    ICurrentActor currentActor,
    ILogger<ArticleByGuidQueryHandler> logger
) : IQueryHandler<ArticleByGuidQuery, ArticleDto?>
{
    public async Task<ArticleDto?> HandleAsync(
        ArticleByGuidQuery query,
        CancellationToken cancellationToken = default)
    {
        logger.QueryByGuidStarted(query.ArticleGuid, currentActor.ActorId);

        var actor = await actorService.GetActorByActorIdAsync(currentActor.ActorId, cancellationToken);

        ActorAssertFound.ThrowNotFoundIfNull(actor, currentActor.ActorId);

        var article = await articleRepository.GetInfoByGuidAsync(
            query.ArticleGuid,
            query.AsOfDateTime,
            childOfAnyOfThesePartitions: actor.PartitionAccessGuids,
            notChildOfAnyPartition: true,
            cancellationToken);

        logger.QueryByGuidCompleted(query.ArticleGuid, currentActor.ActorId, found: article is not null);

        return article;
    }
}
