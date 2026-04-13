using Fargo.Application.Exceptions;
using Fargo.Application.Extensions;
using Fargo.Application.Security;
using Fargo.Application.Storage;
using Fargo.Domain.Repositories;
using Fargo.Domain.Services;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Queries.ArticleQueries;

/// <summary>
/// Represents the result of retrieving an article's image.
/// </summary>
/// <param name="Stream">The image data stream.</param>
/// <param name="ContentType">The MIME content type of the image.</param>
public sealed record ArticleImageResult(Stream Stream, string ContentType);

/// <summary>
/// Query used to retrieve the image associated with an article.
/// </summary>
/// <param name="ArticleGuid">The unique identifier of the article.</param>
public sealed record ArticleImageQuery(
        Guid ArticleGuid
        ) : IQuery<ArticleImageResult?>;

/// <summary>
/// Handles <see cref="ArticleImageQuery"/>.
/// </summary>
public sealed class ArticleImageQueryHandler(
        ActorService actorService,
        IArticleRepository articleRepository,
        IArticleImageStorage imageStorage,
        ICurrentUser currentUser
        ) : IQueryHandler<ArticleImageQuery, ArticleImageResult?>
{
    /// <summary>
    /// Executes the query to retrieve an article's image.
    /// </summary>
    /// <param name="query">The query containing the article identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// An <see cref="ArticleImageResult"/> with the image stream and content type,
    /// or <see langword="null"/> if the article does not exist, is not accessible,
    /// or has no image.
    /// </returns>
    /// <exception cref="UnauthorizedAccessFargoApplicationException">
    /// Thrown when the current actor is not authenticated or inactive.
    /// </exception>
    public async Task<ArticleImageResult?> Handle(
            ArticleImageQuery query,
            CancellationToken cancellationToken = default
            )
    {
        ArgumentNullException.ThrowIfNull(query);

        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        ArticleInformation? info;

        if (actor.IsAdmin || actor.IsSystem)
        {
            info = await articleRepository.GetInfoByGuid(query.ArticleGuid, null, cancellationToken);
        }
        else
        {
            info = await articleRepository.GetInfoByGuidInPartitions(
                query.ArticleGuid,
                actor.PartitionAccesses,
                null,
                cancellationToken);
        }

        if (info is null || !info.HasImage)
        {
            return null;
        }

        // Fetch the full entity to get ImageKey (projection does not expose it for security).
        var article = await articleRepository.GetByGuid(query.ArticleGuid, cancellationToken);

        if (article?.ImageKey is null)
        {
            return null;
        }

        var imageData = await imageStorage.GetAsync(article.ImageKey, cancellationToken);

        if (imageData is null)
        {
            return null;
        }

        return new ArticleImageResult(imageData.Value.Stream, imageData.Value.ContentType);
    }
}
