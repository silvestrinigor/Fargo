using Fargo.Domain.Users;

namespace Fargo.Application.UserGroups;

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
