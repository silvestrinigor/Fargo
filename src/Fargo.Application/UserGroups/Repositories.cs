using Fargo.Core.UserGroups;

namespace Fargo.Application.UserGroups;

public interface IUserGroupQueryRepository
{
    Task<UserGroupDto?> GetInfoByGuid(
        Guid entityGuid,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<UserGroupDto>> GetManyInfo(
        Pagination pagination,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default
    );
}

public static class UserGroupRepositoryExtensions
{
    extension(IUserGroupRepository repository)
    {
        public async Task<UserGroup> GetFoundByGuid(
            Guid userGroupGuid,
            CancellationToken cancellationToken = default
        )
        {
            var group = await repository.GetByGuid(userGroupGuid, cancellationToken)
                ?? throw new UserGroupNotFoundFargoApplicationException(userGroupGuid);

            return group;
        }
    }
}
