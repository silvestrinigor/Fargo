using Fargo.Sdk.Contracts.Permissions;

namespace Fargo.Sdk.Contracts.UserGroups;

/// <summary>Represents a user-group update request.</summary>
public sealed record UserGroupUpdateRequest(
    string? Nameid = null,
    string? Description = null,
    bool? IsActive = null,
    IReadOnlyCollection<PermissionUpdateRequest>? Permissions = null,
    IReadOnlyCollection<Guid>? Partitions = null);
