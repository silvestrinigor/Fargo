using Fargo.Core.Shared.Barcodes;

namespace Fargo.Core.Articles;

/// <summary>
/// Defines the repository contract for managing <see cref="Article"/> entities.
/// </summary>
/// <remarks>
/// Implementations are responsible for retrieving articles from the
/// persistence layer and tracking changes for creation and deletion.
/// Changes are typically committed through a unit of work.
/// </remarks>
public interface IArticleRepository
{
    /// <summary>
    /// Gets an article by its unique identifier.
    /// </summary>
    /// <param name="articleGuid">
    /// The unique identifier of the article.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// The matching <see cref="Article"/> if found; otherwise,
    /// <see langword="null"/>.
    /// </returns>
    Task<Article?> GetByGuidAsync(Guid articleGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether an article is already associated with the specified
    /// EAN-13 barcode.
    /// </summary>
    /// <param name="ean13">
    /// The EAN-13 barcode to search for.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if an article with the specified barcode exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    Task<bool> ExistsByEan13Async(Ean13 ean13, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether the specified article has any associated items.
    /// </summary>
    /// <param name="articleGuid">
    /// The unique identifier of the article.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if one or more items are associated with the
    /// article; otherwise, <see langword="false"/>.
    /// </returns>
    Task<bool> HasItemsAssociatedAsync(Guid articleGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether the specified article is used as a dependency
    /// by another article.
    /// </summary>
    /// <param name="articleGuid">
    /// The unique identifier of the article.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if another article depends on the specified
    /// article; otherwise, <see langword="false"/>.
    /// </returns>
    Task<bool> IsDependencyOfAnotherArticleAsync(Guid articleGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new article to the persistence context.
    /// </summary>
    /// <param name="article">
    /// The article to add.
    /// </param>
    /// <remarks>
    /// The article is tracked by the persistence context. The operation is
    /// not committed until the associated unit of work is completed.
    /// </remarks>
    void Add(Article article);


    /// <summary>
    /// Removes an article from the persistence context.
    /// </summary>
    /// <param name="article">
    /// The article to remove.
    /// </param>
    /// <remarks>
    /// The removal is staged in the persistence context and is not committed
    /// until the associated unit of work is completed.
    /// </remarks>
    void Remove(Article article);
}
