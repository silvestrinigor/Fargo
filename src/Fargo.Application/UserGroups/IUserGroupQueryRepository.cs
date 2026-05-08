namespace Fargo.Application.UserGroups;

public interface IUserGroupQueryRepository
{
    Task<UserGroupDto?> GetInfoByGuid(
        Guid entityGuid,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? insideAnyOfThisPartitions = null,
        bool? notInsideAnyPartition = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<UserGroupDto>> GetManyInfo(
        Pagination pagination,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? insideAnyOfThisPartitions = null,
        bool? notInsideAnyPartition = null,
        CancellationToken cancellationToken = default
    );
}
