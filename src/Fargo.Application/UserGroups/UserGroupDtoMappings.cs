using Fargo.Application.Shared.UserGroups;
using Fargo.Core.UserGroups;
using System.Linq.Expressions;

namespace Fargo.Application.UserGroups;

public static class UserGroupDtoMappings
{
    public static readonly Expression<Func<UserGroup, UserGroupDto>> Projection = userGroup => new UserGroupDto(
        userGroup.Guid,
        userGroup.Nameid,
        userGroup.Description,
        userGroup.Permissions,
        userGroup.Partitions.Select(partition => partition.Guid).ToArray(),
        userGroup.IsActive,
        userGroup.IsAdminUserGroup);
}
