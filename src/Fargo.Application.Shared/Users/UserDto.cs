using Fargo.Core.Shared;
using Fargo.Core.Shared.Actions;

namespace Fargo.Application.Shared.Users;

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
