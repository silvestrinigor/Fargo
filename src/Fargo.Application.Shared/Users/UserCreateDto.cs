using Fargo.Core.Shared;

namespace Fargo.Application.Shared.Users;

public sealed record UserCreateDto(
    Nameid Nameid,
    Password Password,
    FirstName? FirstName = null,
    LastName? LastName = null,
    Description? Description = null,
    IReadOnlyCollection<UserPermissionUpdateDto>? PermissionsToAdd = null,
    TimeSpan? DefaultPasswordExpirationTimeSpan = null,
    IReadOnlyCollection<Guid>? PartitionsToAdd = null,
    IReadOnlyCollection<Guid>? UserGroupsToAdd = null
);
