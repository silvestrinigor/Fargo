using Fargo.Core.Shared;
using Fargo.Core.Shared.Actions;

namespace Fargo.Application.Shared.UserGroups;

public sealed record UserGroupCreateDto(
    Nameid Nameid,
    Description? Description = null,
    bool? IsActive = null,
    IReadOnlyCollection<ActionType>? PermissionsToAdd = null,
    IReadOnlyCollection<Guid>? PartitionsToAdd = null,
    IReadOnlyCollection<Guid>? PartitionAccessesToAdd = null
);
