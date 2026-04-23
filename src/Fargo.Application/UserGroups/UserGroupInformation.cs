using Fargo.Domain;
using Fargo.Domain.Users;

namespace Fargo.Application.UserGroups;

public sealed record UserGroupInformation(
    Guid Guid,
    Nameid Nameid,
    Description Description,
    bool IsActive,
    IReadOnlyCollection<Permission> Permissions
);
