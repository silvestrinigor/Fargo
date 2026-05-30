using Fargo.Core.Articles;
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
    Task<ArticleDto?> GetInfoByGuid(
        Guid entityGuid,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default
    );

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
    Task<ArticleDto?> GetInfoByBarcode(
        Barcode barcode,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default
    );

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
    Task<IReadOnlyCollection<ArticleDto>> GetManyInfo(
        Pagination pagination,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default
    );
}

/// <summary>
/// Provides article repository extensions.
/// </summary>
public static class ArticleRepositoryExtensions
{
    extension(IArticleRepository repository)
    {
        /// <summary>
        /// Retrieves an article by identifier or throws an exception when not found.
        /// </summary>
        /// <param name="articleGuid">
        /// Article unique identifier.
        /// </param>
        /// <param name="cancellationToken">
        /// Cancellation token.
        /// </param>
        /// <returns>
        /// Found article entity.
        /// </returns>
        /// <exception cref="ArticleNotFoundFargoApplicationException">
        /// Thrown when the article does not exist.
        /// </exception>
        public async Task<Article> GetFoundByGuid(
            Guid articleGuid,
            CancellationToken cancellationToken = default
        )
        {
            var article = await repository.GetByGuid(articleGuid, cancellationToken)
                ?? throw new ArticleNotFoundFargoApplicationException(articleGuid);

            return article;
        }
    }
}
