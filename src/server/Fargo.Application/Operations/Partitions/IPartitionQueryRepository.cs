using Fargo.Application.Common;
using Fargo.Application.Operations.Partitions.Dtos;

namespace Fargo.Application.Operations.Partitions;

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
        Sort? sort = null,
        string? search = null,
        CancellationToken cancellationToken = default);
}
