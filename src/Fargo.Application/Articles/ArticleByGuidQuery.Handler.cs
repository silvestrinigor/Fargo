using Fargo.Application.Actors;
using Fargo.Application.Identity;
using Fargo.Application.Shared.Articles;
using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles.Queries;

/// <summary>
/// Handles article queries by identifier.
/// </summary>
/// <remarks>
/// Retrieves an article visible to the current actor.
/// </remarks>
public sealed class ArticleByGuidQueryHandler(
    ActorQueryService actorService,
    IArticleQueryRepository articleRepository,
    ICurrentActor currentActor,
    ILogger<ArticleByGuidQueryHandler> logger
) : IQueryHandler<ArticleByGuidQuery, ArticleDto?>
{
    public async Task<ArticleDto?> HandleAsync(
        ArticleByGuidQuery query,
        CancellationToken cancellationToken = default
    )
    {
        logger.QueryByGuidStarted(query.ArticleGuid, currentActor.ActorId);

        var actor = await actorService.GetActorByActorIdAsync(currentActor.ActorId, cancellationToken);

        ActorAssertFound.ThrowNotAuthorizedIfNull(actor);

        var article = await articleRepository.GetInfoByGuid(
            query.ArticleGuid,
            query.AsOfDateTime,
            childOfAnyOfThesePartitions: actor.PartitionAccessGuids,
            notChildOfAnyPartition: true,
            cancellationToken
        );

        logger.QueryByGuidCompleted(query.ArticleGuid, currentActor.ActorId, article is not null);

        return article;
    }
}
