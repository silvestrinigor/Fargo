using Fargo.Core.Shared;

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
