using Fargo.Core;
using Fargo.Core.Users;
using System.Linq.Expressions;

namespace Fargo.Application.Users;

public sealed record UserDto(
    Guid Guid,
    Nameid Nameid,
    FirstName? FirstName,
    LastName? LastName,
    Description Description,
    TimeSpan? DefaultPasswordExpirationPeriod,
    DateTimeOffset? RequirePasswordChangeAt,
    IReadOnlyCollection<Permission> Permissions,
    IReadOnlyCollection<Guid> Partitions,
    IReadOnlyCollection<Guid> UserGroups,
    bool IsActive,
    Guid? EditedByGuid,
    UserModifiedType ModificationTypes
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

public static class UserDtoMappings
{
    public static readonly Expression<Func<User, UserDto>> Projection = user => new UserDto(
        user.Guid,
        user.Nameid,
        user.FirstName,
        user.LastName,
        user.Description,
        user.DefaultPasswordExpirationPeriod,
        user.RequirePasswordChangeAt,
        user.Permissions.Select(permission => new Permission(permission.Guid, permission.Action)).ToArray(),
        user.Partitions.Select(partition => partition.Guid).ToArray(),
        user.UserGroups.Select(group => group.Guid).ToArray(),
        user.IsActive,
        user.EditedByGuid,
        user.ModificationTypes);
}
