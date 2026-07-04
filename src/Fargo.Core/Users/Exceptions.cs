using Fargo.Core.Shared;

namespace Fargo.Core.Users;

/// <summary>
/// Exception thrown when an attempt is made to modify the permissions
/// of the main administrator user.
/// </summary>
/// <remarks>
/// The main administrator user has fixed permissions that cannot be altered.
/// </remarks>
public sealed class ChangeMainAdminUserPermissionsFargoDomainException()
    : Exception("The permissions of the main administrator user cannot be modified.")
{
}
/// <summary>
/// Exception thrown when an attempt is made to delete the main administrator user.
/// </summary>
/// <remarks>
/// The main administrator user is a critical system entity and cannot be deleted.
/// </remarks>
public sealed class DeleteMainAdminUserFargoDomainException()
    : Exception("The main administrator user cannot be deleted.")
{
}
/// <summary>
/// Exception thrown when a user attempts to change their own permissions.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UserCannotChangeOwnPermissionsFargoDomainException"/> class.
/// </remarks>
/// <param name="userGuid">
/// The identifier of the user who attempted to change their own permissions.
/// </param>
public sealed class UserCannotChangeOwnPermissionsFargoDomainException(Guid userGuid)
    : Exception($"User '{userGuid}' cannot change their own permissions.")
{
    /// <summary>
    /// Gets the identifier of the user who attempted to change their own permissions.
    /// </summary>
    public Guid UserGuid { get; } = userGuid;
}
/// <summary>
/// Exception thrown when a user attempts to delete a user group
/// that they belong to.
/// </summary>
/// <remarks>
/// This rule prevents users from accidentally removing a group
/// that grants their own permissions, which could result in
/// privilege loss or inconsistent authorization state.
/// </remarks>
/// <param name="userGroupGuid">
/// The unique identifier of the user group that the user attempted to delete.
/// </param>
public sealed class UserCannotDeleteParentUserGroupFargoDomainException(Guid userGroupGuid)
    : Exception($"The user cannot delete the user group '{userGroupGuid}' because they belong to it.")
{
    /// <summary>
    /// Gets the unique identifier of the user group that cannot be deleted.
    /// </summary>
    public Guid UserGroupGuid { get; } = userGroupGuid;
}
/// <summary>
/// Exception thrown when a user attempts to delete their own account.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UserCannotDeleteSelfFargoDomainException"/> class.
/// </remarks>
/// <param name="userGuid">
/// The identifier of the user that attempted to delete themselves.
/// </param>
public sealed class UserCannotDeleteSelfFargoDomainException(Guid userGuid)
    : Exception($"User '{userGuid}' cannot delete their own account.")
{
    /// <summary>
    /// Gets the identifier of the user that attempted to delete themselves.
    /// </summary>
    public Guid UserGuid { get; } = userGuid;
}
