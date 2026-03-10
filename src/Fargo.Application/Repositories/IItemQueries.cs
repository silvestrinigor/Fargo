using Fargo.Application.Common;
using Fargo.Application.Models.ItemModels;

namespace Fargo.Application.Repositories
{
    /// <summary>
    /// Provides read operations for <see cref="ItemReadModel"/>.
    /// </summary>
    /// <remarks>
    /// This repository belongs to the query side of the application (CQRS)
    /// and is responsible only for retrieving item data.
    /// It must not modify the state of the system.
    /// </remarks>
    public interface IItemQueries
    {
        /// <summary>
        /// Retrieves a single item by its unique identifier.
        /// </summary>
        /// <param name="entityGuid">
        /// The unique identifier of the item.
        /// </param>
        /// <param name="asOfDateTime">
        /// Optional point in time used to retrieve historical data.
        /// When provided, the result represents the state of the item
        /// as it existed at the specified date and time.
        /// </param>
        /// <param name="cancellationToken">
        /// Token used to cancel the operation.
        /// </param>
        /// <returns>
        /// The <see cref="ItemReadModel"/> if found; otherwise <c>null</c>.
        /// </returns>
        Task<ItemReadModel?> GetByGuid(
                Guid entityGuid,
                DateTimeOffset? asOfDateTime = null,
                CancellationToken cancellationToken = default
                );

        /// <summary>
        /// Retrieves multiple items using filtering and pagination.
        /// </summary>
        /// <param name="pagination">
        /// Pagination parameters used to limit and offset the result set.
        /// </param>
        /// <param name="articleGuid">
        /// Optional identifier used to filter items associated with a specific article.
        /// </param>
        /// <param name="asOfDateTime">
        /// Optional point in time used to retrieve historical data.
        /// When provided, the result represents the state of the items
        /// as they existed at the specified date and time.
        /// </param>
        /// <param name="cancellationToken">
        /// Token used to cancel the operation.
        /// </param>
        /// <returns>
        /// A read-only collection of <see cref="ItemReadModel"/>.
        /// </returns>
        Task<IReadOnlyCollection<ItemReadModel>> GetMany(
                Pagination pagination,
                Guid? articleGuid = null,
                DateTimeOffset? asOfDateTime = null,
                CancellationToken cancellationToken = default
            );
    }
}