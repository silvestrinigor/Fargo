using Fargo.Core.Entities.Actors;
using Fargo.Core.Informations;

namespace Fargo.Application.Dtos.UserGroups;

public sealed record UserGroupUpdateDto(
    Nameid? Nameid = null,
    Description? Description = null,
    bool? IsActive = null,
    Guid? ParentUserGroup = null,
    bool? RemoveParentUserGroup = null,
    IReadOnlyCollection<ActionType>? PermissionsToAdd = null,
    IReadOnlyCollection<ActionType>? PermissionsToRemove = null,
    IReadOnlyCollection<Guid>? PartitionsToAdd = null,
    IReadOnlyCollection<Guid>? PartitionsToRemove = null,
    IReadOnlyCollection<Guid>? PartitionAccessesToAdd = null,
    IReadOnlyCollection<Guid>? PartitionAccessesToRemove = null
);
