using Fargo.Core;
using Fargo.Core.UserGroups;
using Fargo.Core.Users;
using System.Linq.Expressions;

namespace Fargo.Application.UserGroups;

public sealed record UserGroupDto(
    Guid Guid,
    Nameid Nameid,
    Description Description,
    IReadOnlyCollection<Permission> Permissions,
    IReadOnlyCollection<Guid> Partitions,
    bool IsActive,
    Guid? EditedByGuid,
    UserGroupModifiedType ModificationTypes
);

public sealed record UserGroupCreateDto(
    string Nameid,
    Description? Description = null,
    IReadOnlyCollection<UserGroupPermissionUpdateDto>? Permissions = null,
    IReadOnlyCollection<Guid>? Partitions = null
);

public sealed record UserGroupUpdateDto(
    string? Nameid,
    Description? Description,
    bool? IsActive,
    IReadOnlyCollection<UserGroupPermissionUpdateDto>? Permissions,
    IReadOnlyCollection<Guid>? Partitions
);

public sealed record UserGroupPermissionUpdateDto(
    ActionType Action
);

public static class UserGroupDtoMappings
{
    public static readonly Expression<Func<UserGroup, UserGroupDto>> Projection = userGroup => new UserGroupDto(
        userGroup.Guid,
        userGroup.Nameid,
        userGroup.Description,
        userGroup.Permissions.Select(permission => new Permission(permission.Guid, permission.Action)).ToArray(),
        userGroup.Partitions.Select(partition => partition.Guid).ToArray(),
        userGroup.IsActive,
        userGroup.EditedByGuid,
        userGroup.ModificationTypes);
}
