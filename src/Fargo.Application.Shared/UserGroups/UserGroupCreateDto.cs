using Fargo.Core.Shared;

namespace Fargo.Application.Shared.UserGroups;

public sealed record UserGroupCreateDto(
    Nameid Nameid,
    Description? Description = null,
    IReadOnlyCollection<ActionType>? PermissionsToAdd = null,
    IReadOnlyCollection<Guid>? PartitionsToAdd = null,
    IReadOnlyCollection<Guid>? PartitionAccessesToAdd = null
);
