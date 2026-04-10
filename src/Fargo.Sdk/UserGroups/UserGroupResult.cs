namespace Fargo.Sdk.UserGroups;

/// <summary>
/// Represents a user group returned by the API.
/// </summary>
/// <param name="Guid">The unique identifier of the user group.</param>
/// <param name="Nameid">The unique name identifier of the group.</param>
/// <param name="Description">A short description of the group.</param>
/// <param name="IsActive">Whether the user group is currently active.</param>
/// <param name="Permissions">The permissions assigned to this group.</param>
public sealed record UserGroupResult(
    Guid Guid,
    string Nameid,
    string Description,
    bool IsActive,
    IReadOnlyCollection<PermissionResult> Permissions
);
