using Fargo.Sdk.Contracts.Permissions;

namespace Fargo.Sdk.Contracts.UserGroups;

/// <summary>Represents a user-group update request.</summary>
public sealed record UserGroupUpdateDto(
    string? Nameid = null,
    string? Description = null,
    bool? IsActive = null,
    IReadOnlyCollection<PermissionUpdateDto>? Permissions = null);
