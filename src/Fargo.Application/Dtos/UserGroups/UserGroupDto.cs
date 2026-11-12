using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.UserGroups;
using Fargo.Core.Informations;
using System.Linq.Expressions;

namespace Fargo.Application.Dtos.UserGroups;

public sealed record UserGroupDto(
    Guid Guid,
    Nameid Nameid,
    Description Description,
    bool IsActive,
    bool IsAdminUserGroup,
    Guid? ParentUserGroup,
    IReadOnlyCollection<ActionType> Permissions,
    IReadOnlyCollection<Guid> Partitions,
    IReadOnlyCollection<Guid> PartitionAccesses
);

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
