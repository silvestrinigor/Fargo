namespace Fargo.Sdk.UserGroups;

/// <summary>
/// Represents a user group returned by the API.
/// </summary>
public sealed record UserGroupResult(
    Guid Guid,
    string Nameid,
    string Description,
    bool IsActive,
    IReadOnlyCollection<PermissionResult> Permissions
);
