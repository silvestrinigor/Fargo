using Fargo.Application.Exceptions;
using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Extensions;

public static class UserGroupRepositoryExtensions
{
    extension(IUserGroupRepository repository)
    {
        public async Task<UserGroup> GetFoundByGuid(Guid userGroupGuid, CancellationToken cancellationToken = default)
        {
            var group = await repository.GetByGuid(userGroupGuid, cancellationToken)
                ?? throw new UserGroupNotFoundFargoApplicationException(userGroupGuid);

            return group;
        }
    }
}
