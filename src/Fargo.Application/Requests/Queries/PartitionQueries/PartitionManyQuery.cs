using Fargo.Application.Common;
using Fargo.Application.Models.PartitionModels;
using Fargo.Application.Repositories;

namespace Fargo.Application.Requests.Queries.PartitionQueries
{
    /// <summary>
    /// Query used to retrieve multiple partitions.
    /// </summary>
    /// <param name="AsOfDateTime">
    /// Optional point in time used to retrieve historical data.
    /// When provided, the query returns the state of the partitions
    /// as they existed at the specified date and time.
    /// </param>
    /// <param name="Pagination">
    /// Optional pagination parameters used to limit and offset the result set.
    /// If not provided, a default pagination configuration is used.
    /// </param>
    public sealed record PartitionManyQuery(
            DateTimeOffset? AsOfDateTime = null,
            Pagination? Pagination = null
            ) : IQuery<IReadOnlyCollection<PartitionReadModel>>;

    /// <summary>
    /// Handles the execution of <see cref="PartitionManyQuery"/>.
    /// </summary>
    public sealed class PartitionManyQueryHandler(
            IPartitionQueries partitionRepository
            )
        : IQueryHandler<PartitionManyQuery, IReadOnlyCollection<PartitionReadModel>>
    {
        /// <summary>
        /// Executes the query to retrieve multiple partitions.
        /// </summary>
        /// <param name="query">
        /// The query containing the temporal filter and optional pagination parameters.
        /// </param>
        /// <param name="cancellationToken">
        /// Token used to cancel the operation.
        /// </param>
        /// <returns>
        /// A read-only collection of <see cref="PartitionReadModel"/> representing
        /// the partitions that match the specified temporal reference and pagination.
        /// </returns>
        /// <remarks>
        /// If pagination is not provided, the query uses
        /// <see cref="Pagination.First20Pages"/> as the default.
        /// </remarks>
        public async Task<IReadOnlyCollection<PartitionReadModel>> Handle(
                PartitionManyQuery query,
                CancellationToken cancellationToken = default
                )
        {
            var partitions = await partitionRepository.GetMany(
                    query.Pagination ?? Pagination.First20Pages,
                    query.AsOfDateTime,
                    cancellationToken
                    );

            return partitions;
        }
    }
}