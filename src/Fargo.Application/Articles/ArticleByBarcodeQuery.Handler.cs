using Fargo.Application.Common;
using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles;

/// <summary>
/// Handles queries that retrieve an article by its barcode.
/// </summary>
/// <param name="actorService">Resolves the current actor and its partition access.</param>
/// <param name="articleRepository">Provides access to article query data.</param>
/// <param name="currentActor">Provides information about the currently authenticated actor.</param>
/// <param name="logger">Logs the execution of the query.</param>
public sealed class ArticleByBarcodeQueryHandler(
    ActorResolver actorService, IArticleQueryRepository articleRepository,
    ICurrentActor currentActor, ILogger<ArticleByBarcodeQueryHandler> logger
) : IQueryHandler<ArticleByBarcodeQuery, ArticleDto?>
{
    /// <summary>
    /// Retrieves an article matching the requested barcode within the current actor's
    /// accessible partitions.
    /// </summary>
    /// <param name="query">The query containing the barcode to search for.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>
    /// The matching article, or <see langword="null"/> if no accessible article
    /// matches the specified barcode.
    /// </returns>
    /// <exception cref="ActorNotFoundFargoApplicationException">
    /// Thrown when the current actor cannot be found.
    /// </exception>
    public async Task<ArticleDto?> HandleAsync(ArticleByBarcodeQuery query, CancellationToken cancellationToken = default)
    {
        logger.QueryByBarcodeStarted(query.ArticleBarcode, currentActor.Guid, currentActor.ActorType);

        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        var article = await articleRepository.GetInfoByBarcodeAsync(
            query.ArticleBarcode,
            childOfAnyOfThesePartitions: actor.PartitionAccessGuids,
            notChildOfAnyPartition: true, cancellationToken);

        logger.QueryByBarcodeCompleted(query.ArticleBarcode, currentActor.Guid, currentActor.ActorType, article is not null);

        return article;
    }
}
