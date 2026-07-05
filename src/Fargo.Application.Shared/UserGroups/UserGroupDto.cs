using Fargo.Core.Shared;

namespace Fargo.Application.Shared.UserGroups;

public sealed record UserGroupDto(
    Guid Guid,
    Nameid Nameid,
    Description Description,
    IReadOnlyCollection<Permission> Permissions,
    IReadOnlyCollection<Guid> Partitions,
    bool IsActive);
