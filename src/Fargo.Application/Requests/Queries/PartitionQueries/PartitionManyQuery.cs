using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;
using Fargo.Domain.ValueObjects.Entities;

namespace Fargo.Application.Requests.Queries.PartitionQueries
{
    public sealed record PartitionManyQuery(
            DateTimeOffset? AsOfDateTime = null,
            Pagination? Pagination = null
            ) : IQuery<IReadOnlyCollection<PartitionInformation>>;

    public sealed class PartitionManyQueryHandler(
            IPartitionRepository partitionRepository
            )
        : IQueryHandler<PartitionManyQuery, IReadOnlyCollection<PartitionInformation>>
    {
        public async Task<IReadOnlyCollection<PartitionInformation>> Handle(
                PartitionManyQuery query,
                CancellationToken cancellationToken = default
                )
        {
            var partitions = await partitionRepository.GetManyInfo(
                    query.Pagination ?? Pagination.First20Pages,
                    query.AsOfDateTime,
                    cancellationToken
                    );

            return partitions;
        }
    }
}