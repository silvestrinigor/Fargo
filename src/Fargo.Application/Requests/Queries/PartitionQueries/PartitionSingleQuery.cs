using Fargo.Application.Models.PartitionModels;
using Fargo.Application.Repositories;

namespace Fargo.Application.Requests.Queries.PartitionQueries
{
    /// <summary>
    /// Query used to retrieve a single partition by its unique identifier.
    /// </summary>
    /// <param name="PartitionGuid">
    /// The unique identifier of the partition.
    /// </param>
    /// <param name="AsOfDateTime">
    /// Optional point in time used to retrieve historical data.
    /// When provided, the query returns the state of the partition
    /// as it existed at the specified date and time.
    /// </param>
    public sealed record PartitionSingleQuery(
            Guid PartitionGuid,
            DateTimeOffset? AsOfDateTime = null
            ) : IQuery<PartitionReadModel?>;

    /// <summary>
    /// Handles the execution of <see cref="PartitionSingleQuery"/>.
    /// </summary>
    public sealed class PartitionSingleQueryHandler(
            IPartitionQueries repository
            ) : IQueryHandler<PartitionSingleQuery, PartitionReadModel?>
    {
        /// <summary>
        /// Executes the query to retrieve a single partition.
        /// </summary>
        /// <param name="query">The query containing the partition identifier.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>
        /// The <see cref="PartitionReadModel"/> if the partition exists;
        /// otherwise <c>null</c>.
        /// </returns>
        public async Task<PartitionReadModel?> Handle(
                PartitionSingleQuery query,
                CancellationToken cancellationToken = default
                )
            => await repository.GetByGuid(
                    query.PartitionGuid,
                    query.AsOfDateTime,
                    cancellationToken
                    );
    }
}