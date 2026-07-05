using Fargo.Core.Shared;

namespace Fargo.Application.Shared.UserGroups;

public sealed record UserGroupUpdateDto(
    string? Nameid,
    Description? Description,
    bool? IsActive,
    IReadOnlyCollection<UserGroupPermissionUpdateDto>? Permissions,
    IReadOnlyCollection<Guid>? Partitions
);
