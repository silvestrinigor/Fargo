using Fargo.Core.Actors;
using Fargo.Core.Informations;

namespace Fargo.Application.Shared.UserGroups;

public sealed record UserGroupCreateDto(
    Nameid Nameid,
    Description? Description = null,
    bool? IsActive = null,
    Guid? ParentUserGroup = null,
    IReadOnlyCollection<ActionType>? PermissionsToAdd = null,
    IReadOnlyCollection<Guid>? PartitionsToAdd = null,
    IReadOnlyCollection<Guid>? PartitionAccessesToAdd = null
);
