using Fargo.Core.Shared;

namespace Fargo.Application.Shared.Users;

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
    IReadOnlyCollection<Guid>? UserGroupsToAdd = null
);