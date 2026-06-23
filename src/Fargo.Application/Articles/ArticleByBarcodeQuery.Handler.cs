using Fargo.Application.Actors;
using Fargo.Application.Identity;
using Fargo.Application.Shared.Articles;
using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles;

public sealed class ArticleByBarcodeQueryHandler(
    ActorQueryService actorService,
    IArticleQueryRepository articleRepository,
    ICurrentActor currentActor,
    ILogger<ArticleByBarcodeQueryHandler> logger
) : IQueryHandler<ArticleByBarcodeQuery, ArticleDto?>
{
    public async Task<ArticleDto?> HandleAsync(
        ArticleByBarcodeQuery query,
        CancellationToken cancellationToken = default)
    {
        logger.QueryByBarcodeStarted(query.ArticleBarcode, currentActor.ActorId);

        var actor = await actorService.GetActorByActorIdAsync(currentActor.ActorId, cancellationToken);

        ActorAssertFound.ThrowNotAuthorizedIfNull(actor);

        var article = await articleRepository.GetInfoByBarcode(
            query.ArticleBarcode,
            query.AsOfDateTime,
            childOfAnyOfThesePartitions: actor.PartitionAccessGuids,
            notChildOfAnyPartition: true,
            cancellationToken);

        logger.QueryByBarcodeCompleted(query.ArticleBarcode, currentActor.ActorId);

        return article;
    }
}
