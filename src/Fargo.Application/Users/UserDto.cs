using Fargo.Core.Actors;
using Fargo.Core.Informations;

namespace Fargo.Application.Users;

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
