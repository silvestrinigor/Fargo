using Fargo.Core.Shared;
using Fargo.Core.Shared.Actions;

namespace Fargo.Application.Shared.UserGroups;

public sealed record UserGroupDto(
    Guid Guid,
    Nameid Nameid,
    Description Description,
    bool IsActive,
    bool IsAdminUserGroup,
    IReadOnlyCollection<ActionType> Permissions,
    IReadOnlyCollection<Guid> Partitions,
    IReadOnlyCollection<Guid> PartitionAccesses
);
