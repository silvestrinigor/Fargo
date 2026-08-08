using Fargo.Application.Identity;
using Fargo.Application.Shared.Articles;
using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles;

public sealed class ArticleByBarcodeQueryHandler(
    ActorService actorService, IArticleQueryRepository articleRepository,
    ICurrentActor currentActor, ILogger<ArticleByBarcodeQueryHandler> logger
) : IQueryHandler<ArticleByBarcodeQuery, ArticleDto?>
{
    public async Task<ArticleDto?> HandleAsync(
        ArticleByBarcodeQuery query, CancellationToken cancellationToken = default)
    {
        logger.QueryByBarcodeStarted(query.ArticleBarcode, currentActor.Guid);

        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        var article = await articleRepository.GetInfoByBarcodeAsync(
            query.ArticleBarcode,
            childOfAnyOfThesePartitions: actor.PartitionAccessGuids,
            notChildOfAnyPartition: true, cancellationToken);

        logger.QueryByBarcodeCompleted(query.ArticleBarcode, currentActor.Guid, article is not null);

        return article;
    }
}
