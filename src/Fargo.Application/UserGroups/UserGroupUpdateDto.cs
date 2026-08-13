using Fargo.Core.Actors;
using Fargo.Core.Informations;

namespace Fargo.Application.UserGroups;

public sealed record UserGroupUpdateDto(
    Nameid? Nameid,
    Description? Description,
    bool? IsActive,
    Guid? ParentUserGroup,
    bool? RemoveParentUserGroup,
    IReadOnlyCollection<ActionType>? PermissionsToAdd,
    IReadOnlyCollection<ActionType>? PermissionsToRemove,
    IReadOnlyCollection<Guid>? PartitionsToAdd,
    IReadOnlyCollection<Guid>? PartitionsToRemove,
    IReadOnlyCollection<Guid>? PartitionAccessesToAdd,
    IReadOnlyCollection<Guid>? PartitionAccessesToRemove
);
