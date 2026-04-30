using Fargo.Application.Authentication;
using Fargo.Domain;
using Fargo.Domain.Barcodes;

namespace Fargo.Application.Articles;

/// <summary>
/// Query used to retrieve all barcodes associated with an article.
/// </summary>
/// <param name="ArticleGuid">The unique identifier of the article.</param>
public sealed record ArticleBarcodesQuery(
    Guid ArticleGuid
    ) : IQuery<ArticleBarcodes?>;

/// <summary>
/// Handles <see cref="ArticleBarcodesQuery"/>.
/// </summary>
public sealed class ArticleBarcodesQueryHandler(
    ActorService actorService,
    IArticleQueryRepository articleRepository,
    IBarcodeRepository barcodeRepository,
    ICurrentUser currentUser
    ) : IQueryHandler<ArticleBarcodesQuery, ArticleBarcodes?>
{
    /// <summary>
    /// Executes the query to retrieve all barcodes for an article.
    /// </summary>
    /// <param name="query">The query containing the article identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// An <see cref="ArticleBarcodes"/> value, or <see langword="null"/>
    /// if the article does not exist or is not accessible.
    /// </returns>
    /// <exception cref="UnauthorizedAccessFargoApplicationException">
    /// Thrown when the current actor is not authenticated or inactive.
    /// </exception>
    public async Task<ArticleBarcodes?> Handle(
        ArticleBarcodesQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        bool articleExists;

        // TODO: Implement in the repository a function that returns if the article exists insted of return all the information.
        if (actor.IsAdmin || actor.IsSystem)
        {
            articleExists = await articleRepository.GetInfoByGuid(query.ArticleGuid, null, cancellationToken) is not null;
        }
        else
        {
            articleExists = await articleRepository.GetInfoByGuidPublicOrInPartitions(
                query.ArticleGuid,
                actor.PartitionAccesses,
                null,
                cancellationToken) is not null;
        }

        if (!articleExists)
        {
            return null;
        }

        var barcodes = await barcodeRepository.GetByArticleGuid(query.ArticleGuid, cancellationToken);

        return ArticleBarcodes.From(barcodes);
    }
}
