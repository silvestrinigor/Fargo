using Fargo.Core.Shared.Actions;
using Fargo.Core.Shared.Informations;

namespace Fargo.Application.Shared.UserGroups;

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
