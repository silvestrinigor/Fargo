using Fargo.Core.Shared;

namespace Fargo.Application.Shared.Users;

public sealed record UserCreateDto(
    string Nameid,
    string Password,
    FirstName? FirstName = null,
    LastName? LastName = null,
    Description? Description = null,
    IReadOnlyCollection<UserPermissionUpdateDto>? PermissionsToAdd = null,
    TimeSpan? DefaultPasswordExpirationTimeSpan = null,
    IReadOnlyCollection<Guid>? PartitionsToAdd = null,
    IReadOnlyCollection<Guid>? UserGroupsToAdd = null
);
