using Fargo.Application.Authentication;
using Fargo.Domain;
using Fargo.Domain.Articles;

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
    IArticleRepository articleRepository,
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

        var article = await articleRepository.GetByGuid(query.ArticleGuid, cancellationToken);

        if (article is null)
        {
            return null;
        }

        if (!actor.IsAdmin && !actor.IsSystem)
        {
            var hasAccess = !article.Partitions.Any()
                || article.Partitions.Any(p => actor.PartitionAccesses.Contains(p.Guid));

            if (!hasAccess)
            {
                return null;
            }
        }

        return article.Barcodes;
    }
}
