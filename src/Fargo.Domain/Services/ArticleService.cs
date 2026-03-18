using Fargo.Domain.Entities;
using Fargo.Domain.Exceptions;
using Fargo.Domain.Repositories;

namespace Fargo.Domain.Services;

/// <summary>
/// Provides domain operations related to <see cref="Article"/> entities.
/// </summary>
/// <remarks>
/// This service encapsulates business rules involving articles that require
/// coordination with repositories or other domain services, such as deletion
/// constraints and partition-based visibility rules.
/// </remarks>
public class ArticleService(
        IArticleRepository articleRepository
        )
{
    /// <summary>
    /// Deletes the specified <paramref name="article"/> when domain rules allow it.
    /// </summary>
    /// <param name="article">
    /// The article to delete.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="article"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArticleDeleteWithItemsAssociatedFargoDomainException">
    /// Thrown when there are items associated with the article.
    /// </exception>
    /// <remarks>
    /// An article cannot be deleted while there are existing
    /// <see cref="Item"/> instances associated with it.
    /// </remarks>
    public async Task DeleteArticle(
            Article article,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(article);

        var hasItems = await articleRepository.HasItemsAssociated(
            article.Guid,
            cancellationToken
        );

        if (hasItems)
        {
            throw new ArticleDeleteWithItemsAssociatedFargoDomainException(article.Guid);
        }

        articleRepository.Remove(article);
    }

    /// <summary>
    /// Gets an article by its identifier if the specified actor has access to it.
    /// </summary>
    /// <param name="articleGuid">
    /// The unique identifier of the article to retrieve.
    /// </param>
    /// <param name="actor">
    /// The user performing the operation. The actor must have access
    /// to at least one partition associated with the requested article.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// The matching <see cref="Article"/> when it exists and the actor
    /// has access to it; otherwise, <see langword="null"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="actor"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// This method enforces partition-based visibility rules.
    /// An article is returned only when the requesting user has access
    /// to at least one partition associated with it.
    /// </remarks>
    public async Task<Article?> GetArticle(
            Guid articleGuid,
            User actor,
            CancellationToken cancellationToken = default
            )
    {
        ArgumentNullException.ThrowIfNull(actor);

        var article = await articleRepository.GetByGuid(articleGuid, cancellationToken);

        if (article != null && !PartitionService.HasAccess(article, actor))
        {
            return null;
        }

        return article;
    }
}
