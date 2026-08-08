using Fargo.Core.Shared.Actions;
using Fargo.Core.Shared.Informations;

namespace Fargo.Application.Shared.Users;

public sealed record UserCreateDto(
    Nameid Nameid,
    FirstName? FirstName = null,
    LastName? LastName = null,
    Description? Description = null,
    bool? IsActive = null,
    UserAuthenticationCreateDto? Authentication = null,
    IReadOnlyCollection<ActionType>? PermissionsToAdd = null,
    IReadOnlyCollection<Guid>? PartitionsToAdd = null,
    IReadOnlyCollection<Guid>? UserGroupsToAdd = null,
    IReadOnlyCollection<Guid>? PartitionAccessesToAdd = null
);
