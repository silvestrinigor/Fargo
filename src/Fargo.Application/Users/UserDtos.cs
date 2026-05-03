using Fargo.Domain;
using Fargo.Domain.Users;

namespace Fargo.Application.Users;

public sealed record UserDto(
    Guid Guid,
    Nameid Nameid,
    FirstName? FirstName,
    LastName? LastName,
    Description Description,
    TimeSpan DefaultPasswordExpirationPeriod,
    DateTimeOffset RequirePasswordChangeAt,
    IReadOnlyCollection<Permission> Permissions,
    IReadOnlyCollection<Guid> Partitions,
    IReadOnlyCollection<Guid> UserGroups,
    bool IsActive,
    Guid? EditedByGuid
);

public sealed record UserCreateDto(
    string Nameid,
    string Password,
    FirstName? FirstName = null,
    LastName? LastName = null,
    Description? Description = null,
    IReadOnlyCollection<UserPermissionUpdateDto>? Permissions = null,
    TimeSpan? DefaultPasswordExpirationTimeSpan = null,
    IReadOnlyCollection<Guid>? Partitions = null,
    IReadOnlyCollection<Guid>? UserGroups = null
);

public sealed record UserUpdateDto(
    string? Nameid = null,
    FirstName? FirstName = null,
    LastName? LastName = null,
    Description? Description = null,
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

public sealed record UserPasswordUpdateDto(
    string NewPassword,
    string? CurrentPassword = null
);
