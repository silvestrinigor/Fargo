using Fargo.Core.Shared.Barcodes;

namespace Fargo.Core.Articles;

/// <summary>
/// Defines the repository contract for managing <see cref="Article"/> entities.
/// </summary>
public interface IArticleRepository
{
    /// <summary>
    /// Gets an article by its unique identifier.
    /// </summary>
    Task<Article?> GetByGuidAsync(Guid entityGuid, CancellationToken cancellationToken = default);

    Task<bool> ExistsByBarcodeAsync(Barcode barcode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether the specified article has any associated items.
    /// </summary>
    Task<bool> HasItemsAssociatedAsync(Guid articleGuid, CancellationToken cancellationToken = default);

    Task<bool> IsDependenceOfAnotherArticleAsync(Guid articleGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new article to the persistence context.
    /// </summary>
    void Add(Article article);

    /// <summary>
    /// Removes an article from the persistence context.
    /// </summary>
    void Remove(Article article);
}
