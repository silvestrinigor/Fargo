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
    Task<Article?> GetByGuidAsync(
        Guid entityGuid,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByEan13Async(
        Ean13 code,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByEan8Async(
        Ean8 code,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByUpcEAsync(
        UpcE code,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByUpcAAsync(
        UpcA code,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByCode128Async(
        Code128 code,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByCode39Async(
        Code39 code,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByItf14Async(
        Itf14 code,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByGs1128Async(
        Gs1128 code,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByQrCodeAsync(
        QrCode code,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByDataMatrixAsync(
        DataMatrix code,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Determines whether the specified article has any associated items.
    /// </summary>
    Task<bool> HasItemsAssociatedAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default
    );

    Task<bool> IsDependenceOfAnotherArticle(Guid articleGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new article to the persistence context.
    /// </summary>
    void Add(Article article);

    /// <summary>
    /// Removes an article from the persistence context.
    /// </summary>
    void Remove(Article article);
}
