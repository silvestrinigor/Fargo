using Fargo.Core.Entities.Actors;
using Fargo.Core.Informations;
using Fargo.Core.Common;
using Fargo.Core.Entities.Actors.Users;
using System.Linq.Expressions;

namespace Fargo.Application.Operations.Users.Dtos;

public sealed record UserDto(
    Guid Guid,
    Nameid Nameid,
    FirstName? FirstName,
    LastName? LastName,
    Description Description,
    bool IsActive,
    bool IsAdmin,
    UserAuthenticationDto Authentication,
    IReadOnlyCollection<ActionType> Permissions,
    IReadOnlyCollection<Guid> Partitions,
    IReadOnlyCollection<Guid> PartitionAccesses,
    IReadOnlyCollection<Guid> UserGroups
);

public static class UserDtoMappings
{
    public static readonly Expression<Func<User, UserDto>> Projection = user => new UserDto(
        user.ActorGuid,
        user.Actor.Nameid,
        user.FirstName,
        user.LastName,
        user.Actor.Description,
        user.Actor.IsActive,
        user.ActorGuid == FargoCoreWellKnowGuids.AdminUserGuid,
        new UserAuthenticationDto(
            user.Authentication.DefaultPasswordExpirationPeriod,
            user.Authentication.RequirePasswordChangeAt),
        user.Actor.Permissions,
        user.Actor.Partitions.Select(partition => partition.PartitionGuid).ToArray(),
        user.Actor.PartitionAccesses.Select(access => access.PartitionGuid).ToArray(),
        user.UserGroupMemberships.Select(group => group.UserGroupGuid).ToArray()
    );
}
