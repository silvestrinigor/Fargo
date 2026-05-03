using Fargo.Sdk.Contracts.Permissions;

namespace Fargo.Sdk.Contracts.UserGroups;

/// <summary>Represents a user group returned by the API.</summary>
public sealed record UserGroupDto(
    Guid Guid,
    string Nameid,
    string Description,
    bool IsActive,
    IReadOnlyCollection<PermissionDto> Permissions);
