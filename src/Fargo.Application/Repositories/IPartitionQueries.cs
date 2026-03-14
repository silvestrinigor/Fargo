using Fargo.Application.Common;
using Fargo.Application.Models.PartitionModels;

namespace Fargo.Application.Repositories
{
    /// <summary>
    /// Provides read operations for <see cref="PartitionReadModel"/>.
    /// </summary>
    /// <remarks>
    /// This repository is part of the query side of the application (CQRS)
    /// and is responsible only for retrieving partition data.
    /// It must not modify the state of the system.
    /// </remarks>
    public interface IPartitionQueries
    {
        /// <summary>
        /// Retrieves a single partition by its unique identifier.
        /// </summary>
        /// <param name="entityGuid">
        /// The unique identifier of the partition.
        /// </param>
        /// <param name="asOfDateTime">
        /// Optional point in time used to retrieve historical data.
        /// When provided, the result represents the state of the partition
        /// as it existed at the specified date and time.
        /// </param>
        /// <param name="cancellationToken">
        /// Token used to cancel the operation.
        /// </param>
        /// <returns>
        /// The <see cref="PartitionReadModel"/> if found; otherwise <c>null</c>.
        /// </returns>
        Task<PartitionReadModel?> GetByGuid(
                Guid entityGuid,
                DateTimeOffset? asOfDateTime = null,
                CancellationToken cancellationToken = default
                );

        /// <summary>
        /// Retrieves multiple partitions using pagination.
        /// </summary>
        /// <param name="pagination">
        /// Pagination parameters used to limit and offset the result set.
        /// </param>
        /// <param name="asOfDateTime">
        /// Optional point in time used to retrieve historical data.
        /// When provided, the result represents the state of the partitions
        /// as they existed at the specified date and time.
        /// </param>
        /// <param name="cancellationToken">
        /// Token used to cancel the operation.
        /// </param>
        /// <returns>
        /// A read-only collection of <see cref="PartitionReadModel"/>.
        /// </returns>
        Task<IReadOnlyCollection<PartitionReadModel>> GetMany(
                Pagination pagination,
                DateTimeOffset? asOfDateTime = null,
                CancellationToken cancellationToken = default
                );
    }
}