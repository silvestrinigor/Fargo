using Fargo.Application.Identity;
using Fargo.Application.Shared.Articles;
using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles;

public sealed class ArticleByGuidQueryHandler(
    ActorService actorService, IArticleQueryRepository articleRepository,
    ICurrentActor currentActor, ILogger<ArticleByGuidQueryHandler> logger
) : IQueryHandler<ArticleByGuidQuery, ArticleDto?>
{
    public async Task<ArticleDto?> HandleAsync(
        ArticleByGuidQuery query, CancellationToken cancellationToken = default)
    {
        logger.QueryByGuidStarted(query.ArticleGuid, currentActor.Guid);

        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        var article = await articleRepository.GetInfoByGuidAsync(
            query.ArticleGuid,
            childOfAnyOfThesePartitions: actor.PartitionAccessGuids,
            notChildOfAnyPartition: true,
            cancellationToken);

        logger.QueryByGuidCompleted(query.ArticleGuid, currentActor.Guid, found: article is not null);

        return article;
    }
}
