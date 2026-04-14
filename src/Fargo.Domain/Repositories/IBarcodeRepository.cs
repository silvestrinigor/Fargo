using Fargo.Domain.Entities;

namespace Fargo.Domain.Repositories;

/// <summary>
/// Defines data access operations for <see cref="Barcode"/> entities.
/// </summary>
public interface IBarcodeRepository
{
    /// <summary>
    /// Returns all barcodes associated with the specified article.
    /// </summary>
    /// <param name="articleGuid">The unique identifier of the article.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<IReadOnlyCollection<Barcode>> GetByArticleGuid(Guid articleGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the barcode with the specified identifier, or <see langword="null"/> if not found.
    /// </summary>
    /// <param name="barcodeGuid">The unique identifier of the barcode.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<Barcode?> GetByGuid(Guid barcodeGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Schedules the specified barcode for insertion on the next unit-of-work commit.
    /// </summary>
    void Add(Barcode barcode);

    /// <summary>
    /// Schedules the specified barcode for deletion on the next unit-of-work commit.
    /// </summary>
    void Remove(Barcode barcode);
}
