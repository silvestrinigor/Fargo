using Fargo.Application.Shared.Articles;
using Fargo.Core.Shared.Barcodes;

namespace Fargo.Application.Articles;

/// <summary>
/// Provides article query operations.
/// </summary>
public interface IArticleQueryRepository
{
    /// <summary>
    /// Retrieves article information by identifier.
    /// </summary>
    /// <param name="entityGuid">
    /// Article unique identifier.
    /// </param>
    /// <param name="asOfDateTime">
    /// Temporal query date.
    /// </param>
    /// <param name="childOfAnyOfThesePartitions">
    /// Filters articles inside the provided partitions.
    /// </param>
    /// <param name="notChildOfAnyPartition">
    /// Indicates whether articles without partitions should be included.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// Article information if found; otherwise null.
    /// </returns>
    Task<ArticleDto?> GetInfoByGuidAsync(
        Guid entityGuid,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves article information by barcode.
    /// </summary>
    /// <param name="articleBarcode">
    /// Article barcode information.
    /// </param>
    /// <param name="asOfDateTime">
    /// Temporal query date.
    /// </param>
    /// <param name="childOfAnyOfThesePartitions">
    /// Filters articles inside the provided partitions.
    /// </param>
    /// <param name="notChildOfAnyPartition">
    /// Indicates whether articles without partitions should be included.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// Article information if found; otherwise null.
    /// </returns>
    Task<ArticleDto?> GetInfoByBarcodeAsync(
        Barcode barcode,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves multiple article information records.
    /// </summary>
    /// <param name="pagination">
    /// Pagination configuration.
    /// </param>
    /// <param name="asOfDateTime">
    /// Temporal query date.
    /// </param>
    /// <param name="childOfAnyOfThesePartitions">
    /// Filters articles inside the provided partitions.
    /// </param>
    /// <param name="notChildOfAnyPartition">
    /// Indicates whether articles without partitions should be included.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// Collection of article information.
    /// </returns>
    Task<IReadOnlyCollection<ArticleDto>> GetManyInfoAsync(
        Pagination pagination,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default);
}
