using Fargo.Core.Actors;
using Fargo.Core.Informations;

namespace Fargo.Application.UserGroups;

public sealed record UserGroupDto(
    Guid Guid,
    Nameid Nameid,
    Description Description,
    bool IsActive,
    bool IsAdminUserGroup,
    Guid? ParentPartition,
    IReadOnlyCollection<ActionType> Permissions,
    IReadOnlyCollection<Guid> Partitions,
    IReadOnlyCollection<Guid> PartitionAccesses
);
