using Fargo.Core.Shared;
using Fargo.Core.Shared.Actions;

namespace Fargo.Application.Shared.Users;

public sealed record UserUpdateDto(
    Nameid? Nameid = null,
    FirstName? FirstName = null,
    LastName? LastName = null,
    Description? Description = null,
    bool? IsActive = null,
    UserAuthenticationUpdateDto? Authentication = null,
    IReadOnlyCollection<ActionType>? PermissionsToAdd = null,
    IReadOnlyCollection<ActionType>? PermissionsToRemove = null,
    IReadOnlyCollection<Guid>? PartitionsToAdd = null,
    IReadOnlyCollection<Guid>? PartitionsToRemove = null,
    IReadOnlyCollection<Guid>? UserGroupsToAdd = null,
    IReadOnlyCollection<Guid>? UserGroupsToRemove = null,
    IReadOnlyCollection<Guid>? PartitionAccessesToAdd = null,
    IReadOnlyCollection<Guid>? PartitionAccessesToRemove = null
);
