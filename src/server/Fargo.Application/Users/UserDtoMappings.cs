using Fargo.Core.Entities.Actors.Users;
using System.Linq.Expressions;

namespace Fargo.Application.Users;

public static class UserDtoMappings
{
    public static readonly Expression<Func<User, UserDto>> Projection = user => new UserDto(
        user.ActorGuid,
        user.Actor.Nameid,
        user.FirstName,
        user.LastName,
        user.Actor.Description,
        user.Actor.IsActive,
        user.ActorGuid == Fargo.Core.Common.FargoCoreWellKnowGuids.AdminUserGuid,
        new UserAuthenticationDto(
            user.Authentication.DefaultPasswordExpirationPeriod,
            user.Authentication.RequirePasswordChangeAt),
        user.Actor.Permissions,
        user.Actor.Partitions.Select(partition => partition.PartitionGuid).ToArray(),
        user.Actor.PartitionAccesses.Select(access => access.PartitionGuid).ToArray(),
        user.UserGroupMemberships.Select(group => group.UserGroupGuid).ToArray()
    );
}
