using Fargo.Api.Contracts.Permissions;

namespace Fargo.Api.Contracts.UserGroups;

/// <summary>Represents a user-group update request.</summary>
public sealed record UserGroupUpdateDto(
    string? Nameid = null,
    string? Description = null,
    bool? IsActive = null,
    IReadOnlyCollection<PermissionUpdateDto>? Permissions = null);
