using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;

namespace Fargo.Domain.Services.PartitionServices
{
    public class PartitionGetService(
            IPartitionRepository partitionRepository
            )
    {
        public async Task<Partition?> GetPartition(
                User actor,
                Guid partitionGuid,
                CancellationToken cancellationToken = default
                )
        {
            var partition = await partitionRepository.GetByGuid(
                    partitionGuid,
                    [.. actor.PartitionsAccesses.Select(p => p.Guid)],
                    cancellationToken
                    );

            return partition;
        }
    }
}