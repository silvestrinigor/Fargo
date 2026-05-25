namespace Fargo.HttpContracts;

public sealed record UserDto(
    Guid Guid,
    string Nameid,
    string? FirstName,
    string? LastName,
    string Description,
    TimeSpan? DefaultPasswordExpirationPeriod,
    DateTimeOffset? RequirePasswordChangeAt,
    IReadOnlyCollection<PermissionDto> Permissions,
    IReadOnlyCollection<Guid> Partitions,
    IReadOnlyCollection<Guid> UserGroups,
    bool IsActive,
    Guid? EditedByGuid,
    UserModifiedType ModificationTypes
);

public sealed record UserCreateRequest(
    string Nameid,
    string Password,
    string? FirstName = null,
    string? LastName = null,
    string? Description = null,
    IReadOnlyCollection<UserPermissionUpdateDto>? Permissions = null,
    TimeSpan? DefaultPasswordExpirationTimeSpan = null,
    IReadOnlyCollection<Guid>? Partitions = null,
    IReadOnlyCollection<Guid>? UserGroups = null
);

public sealed record UserUpdateRequest(
    string? Nameid = null,
    string? FirstName = null,
    string? LastName = null,
    string? Description = null,
    string? Password = null,
    bool? IsActive = null,
    IReadOnlyCollection<UserPermissionUpdateDto>? Permissions = null,
    TimeSpan? DefaultPasswordExpirationPeriod = null,
    IReadOnlyCollection<Guid>? Partitions = null,
    IReadOnlyCollection<Guid>? UserGroups = null
);

public sealed record UserPermissionUpdateDto(
    ActionType Action
);
