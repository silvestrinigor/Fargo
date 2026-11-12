using Fargo.Core.Entities.Actors;
using Fargo.Core.Informations;

namespace Fargo.Application.Operations.Users.Dtos;

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
