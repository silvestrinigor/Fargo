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
    Task<Article?> GetByGuid(
        Guid entityGuid,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByBarcode(
        Ean13 code,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByBarcode(
        Ean8 code,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByBarcode(
        UpcE code,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByBarcode(
        UpcA code,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByBarcode(
        Code128 code,
        CancellationToken cancellationToken = default
    );


    Task<bool> ExistsByBarcode(
        Code39 code,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByBarcode(
        Itf14 code,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByBarcode(
        Gs1128 code,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByBarcode(
        QrCode code,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByBarcode(
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
