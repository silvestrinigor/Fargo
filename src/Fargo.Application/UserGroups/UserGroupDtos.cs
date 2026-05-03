using Fargo.Domain;
using Fargo.Domain.Users;

namespace Fargo.Application.UserGroups;

public sealed record UserGroupDto(
    Guid Guid,
    Nameid Nameid,
    Description Description,
    IReadOnlyCollection<Permission> Permissions,
    IReadOnlyCollection<Guid> Partitions,
    bool IsActive,
    Guid? EditedByGuid
);

public sealed record UserGroupCreateDto(
    string Nameid,
    Description? Description = null,
    IReadOnlyCollection<UserGroupPermissionUpdateDto>? Permissions = null,
    IReadOnlyCollection<Guid>? Partitions = null
);

public sealed record UserGroupUpdateDto(
    string? Nameid,
    Description? Description,
    bool? IsActive,
    IReadOnlyCollection<UserGroupPermissionUpdateDto>? Permissions,
    IReadOnlyCollection<Guid>? Partitions
);

public sealed record UserGroupPermissionUpdateDto(
    ActionType Action
);
