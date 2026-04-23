using Fargo.Application.Partitions;
using Fargo.Domain;

namespace Fargo.Application.Items;

public interface IItemQueryRepository
{
    Task<ItemInformation?> GetInfoByGuid(
        Guid entityGuid,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<ItemInformation>> GetManyInfo(
        Pagination pagination,
        Guid? articleGuid = null,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<ItemInformation>> GetManyInfoWithNoPartition(
        Pagination pagination,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default
    );

    Task<ItemInformation?> GetInfoByGuidInPartitions(
        Guid entityGuid,
        IReadOnlyCollection<Guid> partitionGuids,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<ItemInformation>> GetManyInfoInPartitions(
        Pagination pagination,
        IReadOnlyCollection<Guid> partitionGuids,
        Guid? articleGuid = null,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<PartitionInformation>?> GetPartitions(
        Guid entityGuid,
        IReadOnlyCollection<Guid>? partitionFilter = null,
        CancellationToken cancellationToken = default
    );
}
