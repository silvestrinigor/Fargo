namespace Fargo.HttpContracts;

public sealed record UserGroupDto(
    Guid Guid,
    string Nameid,
    string Description,
    IReadOnlyCollection<PermissionDto> Permissions,
    IReadOnlyCollection<Guid> Partitions,
    bool IsActive,
    Guid? EditedByGuid,
    UserGroupModifiedType ModificationTypes
);

public sealed record UserGroupCreateRequest(
    string Nameid,
    string? Description = null,
    IReadOnlyCollection<UserGroupPermissionUpdateDto>? Permissions = null,
    IReadOnlyCollection<Guid>? Partitions = null
);

public sealed record UserGroupUpdateRequest(
    string? Nameid,
    string? Description,
    bool? IsActive,
    IReadOnlyCollection<UserGroupPermissionUpdateDto>? Permissions,
    IReadOnlyCollection<Guid>? Partitions
);

public sealed record UserGroupPermissionUpdateDto(
    ActionType Action
);
