namespace Fargo.Domain.Articles;

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
}
