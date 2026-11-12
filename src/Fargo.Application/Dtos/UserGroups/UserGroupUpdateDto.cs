using Fargo.Core.Entities.Actors;
using Fargo.Core.Informations;

namespace Fargo.Application.Dtos.UserGroups;

public sealed record UserGroupUpdateDto(
    Nameid? Nameid = null,
    Description? Description = null,
    Guid? ParentUserGroup = null,
    bool? RemoveParentUserGroup = null,
    IReadOnlyCollection<Guid>? PartitionsToAdd = null,
    IReadOnlyCollection<Guid>? PartitionsToRemove = null,
    IReadOnlyCollection<Guid>? PartitionAccessesToAdd = null,
    IReadOnlyCollection<Guid>? PartitionAccessesToRemove = null
);
