using Fargo.Application.Partitions;
using Fargo.Domain;

namespace Fargo.Application.UserGroups;

public interface IUserGroupQueryRepository
{
    Task<UserGroupInformation?> GetInfoByGuid(
        Guid entityGuid,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<UserGroupInformation>> GetManyInfo(
        Pagination pagination,
        Guid? userGuid = null,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default
    );

    Task<UserGroupInformation?> GetInfoByGuidInPartitions(
        Guid entityGuid,
        IReadOnlyCollection<Guid> partitionGuids,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<UserGroupInformation>> GetManyInfoInPartitions(
        Pagination pagination,
        IReadOnlyCollection<Guid> partitionGuids,
        Guid? userGuid = null,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<PartitionInformation>?> GetPartitions(
        Guid entityGuid,
        IReadOnlyCollection<Guid>? partitionFilter = null,
        CancellationToken cancellationToken = default
    );
}
