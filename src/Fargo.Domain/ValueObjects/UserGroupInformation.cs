namespace Fargo.Domain.ValueObjects;

/// <summary>
/// Represents a lightweight information projection of a user group.
/// </summary>
/// <remarks>
/// This value object contains the essential data required to reference a user group
/// without loading the full aggregate. It is typically used in queries, listings,
/// or authorization contexts where only group metadata and permissions are needed.
/// </remarks>
/// <param name="Guid">
/// The unique identifier of the user group.
/// </param>
/// <param name="Nameid">
/// The unique name identifier of the user group.
/// </param>
/// <param name="Description">
/// A short description explaining the purpose of the user group.
/// </param>
/// <param name="IsActive">
/// Indicates whether the user group is currently active and can be assigned to users.
/// </param>
/// <param name="Permissions">
/// The collection of permissions granted to this user group.
/// </param>
public sealed record UserGroupInformation(
    Guid Guid,
    Nameid Nameid,
    Description Description,
    bool IsActive,
    IReadOnlyCollection<Permission> Permissions
);
