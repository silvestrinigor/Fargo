using Fargo.Core.Shared;
using Fargo.Core.Shared.Actions;

namespace Fargo.Application.Shared.UserGroups;

public sealed record UserGroupUpdateDto(
    Nameid? Nameid,
    Description? Description,
    bool? IsActive,
    IReadOnlyCollection<ActionType>? PermissionsToAdd,
    IReadOnlyCollection<ActionType>? PermissionsToRemove,
    IReadOnlyCollection<Guid>? PartitionsToAdd,
    IReadOnlyCollection<Guid>? PartitionsToRemove,
    IReadOnlyCollection<Guid>? PartitionAccessesToAdd,
    IReadOnlyCollection<Guid>? PartitionAccessesToRemove
);
