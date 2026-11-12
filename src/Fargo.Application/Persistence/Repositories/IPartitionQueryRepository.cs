using Fargo.Application.Common.Paging;
using Fargo.Application.Dtos.Partitions;

namespace Fargo.Application.Persistence.Repositories;

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
