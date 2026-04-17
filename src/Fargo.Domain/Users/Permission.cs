namespace Fargo.Domain.Users;

/// <summary>
/// Represents a permission granted to a user or role within the system.
/// </summary>
/// <remarks>
/// A permission defines an action that can be performed in the system.
/// This value object is typically used to represent authorization rules
/// associated with users, groups, or other security-related entities.
/// </remarks>
/// <param name="Guid">
/// The unique identifier of the permission.
/// </param>
/// <param name="Action">
/// The action that the permission allows to be performed.
/// </param>
public sealed record Permission(
    Guid Guid,
    ActionType Action
);
