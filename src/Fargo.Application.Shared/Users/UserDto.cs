using Fargo.Core.Shared;

namespace Fargo.Application.Shared.Users;

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
    bool IsActive);
