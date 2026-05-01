using Fargo.Api.Contracts.Permissions;

namespace Fargo.Api.Contracts.UserGroups;

/// <summary>Represents a user group returned by the API.</summary>
public sealed record UserGroupDto(
    Guid Guid,
    string Nameid,
    string Description,
    bool IsActive,
    IReadOnlyCollection<PermissionDto> Permissions);
