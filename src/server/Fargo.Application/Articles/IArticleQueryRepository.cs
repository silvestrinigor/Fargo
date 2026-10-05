using Fargo.Application.Common;
using Fargo.Core.Barcodes;

namespace Fargo.Application.Articles;

/// <summary>
/// Provides article query operations.
/// </summary>
public interface IArticleQueryRepository
{
    /// <summary>
    /// Retrieves article information by identifier.
    /// </summary>
    /// <param name="articleGuid">
    /// Article unique identifier.
    /// </param>
    /// <param name="childOfAnyOfThesePartitions">
    /// Filters articles inside the provided partitions.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// Article information if found; otherwise null.
    /// </returns>
    Task<ArticleDto?> GetInfoByGuidAsync(
        Guid articleGuid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves article information by barcode.
    /// </summary>
    /// <param name="articleBarcode">
    /// Article barcode information.
    /// </param>
    /// <param name="childOfAnyOfThesePartitions">
    /// Filters articles inside the provided partitions.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// Article information if found; otherwise null.
    /// </returns>
    Task<ArticleDto?> GetInfoByBarcodeAsync(
        Barcode articleBarcode,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default
    );

    Task<ArticleInventoryDto> GetInventoryInfoByGuidAsync(
        Guid articleGuid,
        IReadOnlyCollection<Guid>? insideItemContainerGuids = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves multiple article information records.
    /// </summary>
    /// <param name="pagination">
    /// Pagination configuration.
    /// </param>
    /// <param name="childOfAnyOfThesePartitions">
    /// Filters articles inside the provided partitions.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// Collection of article information ordered by guid.
    /// </returns>
    Task<IReadOnlyCollection<ArticleDto>> GetManyInfoOrderedByGuidAsync(
        Pagination pagination,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Checks if an article exists by its unique identifier within the specified partitions.
    /// </summary>
    /// <param name="articleGuid">
    /// Article unique identifier to check for existence.
    /// </param>
    /// <param name="childOfAnyOfThesePartitions">
    /// Filters articles inside the provided partitions. If null or empty, no partition filtering is applied.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token to observe while waiting for the task to complete.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains true if the article exists;
    /// otherwise false.
    /// </returns>
    Task<bool> ExistByGuidAsync(
        Guid articleGuid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default
    );
}
