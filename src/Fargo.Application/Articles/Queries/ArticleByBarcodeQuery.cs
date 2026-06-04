using Fargo.Application.Identity;
using Fargo.Application.Shared.Articles;
using Fargo.Core.Shared.Barcodes;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles.Queries;

/// <summary>
/// Query used to retrieve an article by barcode.
/// </summary>
/// <param name="ArticleBarcode">
/// Article barcode information.
/// </param>
/// <param name="AsOfDateTime">
/// Temporal query date.
/// </param>
public sealed record ArticleByBarcodeQuery(
    Barcode ArticleBarcode,
    DateTimeOffset? AsOfDateTime = null
) : IQuery<ArticleDto?>;

/// <summary>
/// Handles article queries by barcode.
/// </summary>
/// <remarks>
/// Retrieves an article using barcode information.
/// </remarks>
public sealed class ArticleByBarcodeQueryHandler(
    IArticleQueryRepository articleRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<ArticleByBarcodeQueryHandler> logger
) : IQueryHandler<ArticleByBarcodeQuery, ArticleDto?>
{
    public async Task<ArticleDto?> Handle(
        ArticleByBarcodeQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Article barcode query started for barcode type {BarcodeType} by actor {ActorGuid}.",
                query.ArticleBarcode.Format,
                actor.ActorGuid);
        }

        var article = await articleRepository.GetInfoByBarcode(
            query.ArticleBarcode,
            query.AsOfDateTime,
            childOfAnyOfThesePartitions: actor.PartitionAccesses,
            notChildOfAnyPartition: true,
            cancellationToken
        );

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Article barcode query completed for barcode type {BarcodeType} by actor {ActorGuid}. Found: {Found}.",
                query.ArticleBarcode.Format,
                actor.ActorGuid,
                article is not null);
        }

        return article;
    }
}
