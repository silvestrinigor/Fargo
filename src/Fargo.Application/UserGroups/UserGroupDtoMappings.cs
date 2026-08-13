using Fargo.Core.UserGroups;
using System.Linq.Expressions;

namespace Fargo.Application.UserGroups;

public static class UserGroupDtoMappings
{
    public static readonly Expression<Func<UserGroup, UserGroupDto>> Projection = userGroup => new UserGroupDto(
        userGroup.Guid,
        userGroup.Nameid,
        userGroup.Description,
        userGroup.IsActive,
        userGroup.IsAdministrators,
        userGroup.ParentUserGroupGuid,
        userGroup.Permissions,
        userGroup.Partitions.Select(partition => partition.PartitionGuid).ToArray(),
        userGroup.PartitionAccesses.Select(partition => partition.PartitionGuid).ToArray()
    );
}
