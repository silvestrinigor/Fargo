using Fargo.Application.Shared.Users;
using Fargo.Core.Shared;
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
        user.DefaultPasswordExpirationPeriod,
        user.RequirePasswordChangeAt,
        user.Permissions.Select(permission => new Permission(permission.Guid, permission.Action)).ToArray(),
        user.Partitions.Select(partition => partition.Guid).ToArray(),
        user.UserGroups.Select(group => group.Guid).ToArray(),
        user.IsActive,
        user.EditedByActorGuid);
}
