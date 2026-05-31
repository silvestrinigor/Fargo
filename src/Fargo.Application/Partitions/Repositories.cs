using Fargo.Core.Partitions;
using Fargo.Application.Shared.Partitions;

namespace Fargo.Application.Partitions;

public interface IPartitionQueryRepository
{
    Task<PartitionDto?> GetInfoByGuid(
        Guid entityGuid,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<PartitionDto>> GetManyInfo(
        Pagination pagination,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default
    );
}

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
