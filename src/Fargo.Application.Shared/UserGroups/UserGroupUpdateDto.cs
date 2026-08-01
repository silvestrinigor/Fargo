using Fargo.Core.Shared;

namespace Fargo.Application.Shared.UserGroups;

public sealed record UserGroupUpdateDto(
    string? Nameid,
    Description? Description,
    bool? IsActive,
    IReadOnlyCollection<ActionType>? Permissions,
    IReadOnlyCollection<Guid>? Partitions
);
