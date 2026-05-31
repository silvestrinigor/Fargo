using Fargo.Core.UserGroups;
using Fargo.Core.Shared;
using Fargo.Application.Shared.UserGroups;
using System.Linq.Expressions;

namespace Fargo.Application.UserGroups;

public static class UserGroupDtoMappings
{
    public static readonly Expression<Func<UserGroup, UserGroupDto>> Projection = userGroup => new UserGroupDto(
        userGroup.Guid,
        userGroup.Nameid,
        userGroup.Description,
        userGroup.Permissions.Select(permission => new Permission(permission.Guid, permission.Action)).ToArray(),
        userGroup.Partitions.Select(partition => partition.Guid).ToArray(),
        userGroup.IsActive,
        userGroup.EditedByGuid);
}
