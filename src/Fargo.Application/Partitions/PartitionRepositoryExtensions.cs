using Fargo.Domain.Partitions;

namespace Fargo.Application.Partitions;

public static class PartitionRepositoryExtensions
{
    extension(IPartitionRepository repository)
    {
        public async Task<Partition> GetFoundByGuid(
            Guid partitionGuid,
            CancellationToken cancellationToken = default
        )
        {
            var partition = await repository.GetByGuid(partitionGuid, cancellationToken)
                ?? throw new PartitionNotFoundFargoApplicationException(partitionGuid);

            return partition;
        }
    }
}
