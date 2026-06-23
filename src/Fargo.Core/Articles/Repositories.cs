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

    Task<bool> ExistsByEan13(
        Ean13 code,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByEan8(
        Ean8 code,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByUpcE(
        UpcE code,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByUpcA(
        UpcA code,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByCode128(
        Code128 code,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByCode39(
        Code39 code,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByItf14(
        Itf14 code,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByGs1128(
        Gs1128 code,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByQrCode(
        QrCode code,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByDataMatrix(
        DataMatrix code,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Determines whether the specified article has any associated items.
    /// </summary>
    Task<bool> HasItemsAssociated(
        Guid articleGuid,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Adds a new article to the persistence context.
    /// </summary>
    void Add(Article article);

    /// <summary>
    /// Removes an article from the persistence context.
    /// </summary>
    void Remove(Article article);
}
