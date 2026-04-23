using Fargo.Domain;
using Fargo.Domain.Users;

namespace Fargo.Application.Users;

public sealed record UserInformation(
    Guid Guid,
    Nameid Nameid,
    FirstName? FirstName,
    LastName? LastName,
    Description Description,
    TimeSpan DefaultPasswordExpirationPeriod,
    DateTimeOffset RequirePasswordChangeAt,
    bool IsActive,
    IReadOnlyCollection<Permission> Permissions,
    IReadOnlyCollection<Guid> PartitionAccesses,
    Guid? EditedByGuid = null
);
