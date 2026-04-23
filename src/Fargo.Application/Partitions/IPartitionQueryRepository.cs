using Fargo.Domain;

namespace Fargo.Application.Partitions;

public interface IPartitionQueryRepository
{
    Task<PartitionInformation?> GetInfoByGuid(
        Guid entityGuid,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<PartitionInformation>> GetManyInfo(
        Pagination pagination,
        Guid? parentPartitionGuid = null,
        DateTimeOffset? asOfDateTime = null,
        bool rootOnly = false,
        string? search = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<PartitionInformation>> GetManyInfoByGuids(
        IReadOnlyCollection<Guid> partitionGuids,
        Pagination pagination,
        Guid? parentPartitionGuid = null,
        DateTimeOffset? asOfDateTime = null,
        bool rootOnly = false,
        string? search = null,
        CancellationToken cancellationToken = default
    );
}
