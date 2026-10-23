using Fargo.Application.Common;

namespace Fargo.Application.Partitions;

public interface IPartitionQueryRepository
{
    Task<PartitionDto?> GetInfoByGuid(
        Guid partitionGuid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default);

    Task<PagedResult<PartitionDto>> GetManyInfo(
        Pagination pagination,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default);
}
