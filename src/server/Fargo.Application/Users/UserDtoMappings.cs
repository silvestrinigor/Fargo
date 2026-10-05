using Fargo.Core.Users;
using System.Linq.Expressions;

namespace Fargo.Application.Users;

public static class UserDtoMappings
{
    public static readonly Expression<Func<User, UserDto>> Projection = user => new UserDto(
        user.Guid,
        user.Nameid,
        user.FirstName,
        user.LastName,
        user.Description,
        user.IsActive,
        user.IsAdmin,
        new UserAuthenticationDto(
            user.Authentication.DefaultPasswordExpirationPeriod,
            user.Authentication.RequirePasswordChangeAt),
        user.Permissions,
        user.Partitions.Select(partition => partition.PartitionGuid).ToArray(),
        user.PartitionAccesses.Select(partition => partition.PartitionGuid).ToArray(),
        user.UserGroupMemberships.Select(group => group.UserGroupGuid).ToArray()
    );
}
