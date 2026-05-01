using Fargo.Sdk.Contracts.Permissions;

namespace Fargo.Sdk.Contracts.UserGroups;

/// <summary>Represents the user-group payload inside a user-group create request.</summary>
public sealed record UserGroupCreateDto(
    string Nameid,
    string? Description = null,
    IReadOnlyCollection<PermissionUpdateRequest>? Permissions = null,
    Guid? FirstPartition = null);

/// <summary>Represents a user-group create request.</summary>
public sealed record UserGroupCreateRequest(UserGroupCreateDto UserGroup);

/// <summary>Represents a user-group update request.</summary>
public sealed record UserGroupUpdateRequest(
    string? Nameid = null,
    string? Description = null,
    bool? IsActive = null,
    IReadOnlyCollection<PermissionUpdateRequest>? Permissions = null);
