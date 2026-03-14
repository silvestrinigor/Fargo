using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects.Entities;

namespace Fargo.Application.Requests.Queries.PartitionQueries
{
    public sealed record PartitionSingleQuery(
            Guid PartitionGuid,
            DateTimeOffset? AsOfDateTime = null
            ) : IQuery<PartitionInformation?>;

    public sealed class PartitionSingleQueryHandler(
            IPartitionRepository partitionRepository
            ) : IQueryHandler<PartitionSingleQuery, PartitionInformation?>
    {
        public async Task<PartitionInformation?> Handle(
                PartitionSingleQuery query,
                CancellationToken cancellationToken = default
                )
        {
            return await partitionRepository.GetInfoByGuid(
                    query.PartitionGuid,
                    query.AsOfDateTime,
                    cancellationToken
                    );
        }
    }
}