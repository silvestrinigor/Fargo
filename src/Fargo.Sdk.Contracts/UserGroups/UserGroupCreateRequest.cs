using Fargo.Sdk.Contracts.Permissions;

namespace Fargo.Sdk.Contracts.UserGroups;

/// <summary>Represents the user-group payload inside a user-group create request.</summary>
public sealed record UserGroupCreateRequest(
    string Nameid,
    string? Description = null,
    IReadOnlyCollection<PermissionUpdateRequest>? Permissions = null,
    IReadOnlyCollection<Guid>? Partitions = null);
