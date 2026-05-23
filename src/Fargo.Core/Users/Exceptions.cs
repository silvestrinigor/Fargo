namespace Fargo.Core.Users;

/// <summary>
/// Exception thrown when an attempt is made to modify the permissions
/// of the main administrator user.
/// </summary>
/// <remarks>
/// The main administrator user has fixed permissions that cannot be altered.
/// </remarks>
public sealed class ChangeMainAdminUserPermissionsFargoDomainException()
    : FargoDomainException("The permissions of the main administrator user cannot be modified.")
{
}
/// <summary>
/// Exception thrown when an attempt is made to delete the main administrator user.
/// </summary>
/// <remarks>
/// The main administrator user is a critical system entity and cannot be deleted.
/// </remarks>
public sealed class DeleteMainAdminUserFargoDomainException()
    : FargoDomainException("The main administrator user cannot be deleted.")
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
    : FargoDomainException($"User '{userGuid}' cannot change their own permissions.")
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
    : FargoDomainException($"The user cannot delete the user group '{userGroupGuid}' because they belong to it.")
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
    : FargoDomainException($"User '{userGuid}' cannot delete their own account.")
{
    /// <summary>
    /// Gets the identifier of the user that attempted to delete themselves.
    /// </summary>
    public Guid UserGuid { get; } = userGuid;
}
/// <summary>
/// Exception thrown when an operation is attempted with an inactive user.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UserInactiveFargoDomainException"/> class.
/// </remarks>
/// <param name="userGuid">
/// The identifier of the inactive user.
/// </param>
public sealed class UserInactiveFargoDomainException(Guid userGuid)
    : FargoDomainException($"The user '{userGuid}' is inactive and cannot perform this operation.")
{
    /// <summary>
    /// Gets the GUID of the user that is inactive.
    /// </summary>
    public Guid UserGuid { get; } = userGuid;
}
/// <summary>
/// Represents an error that occurs when attempting to create
/// a <c>User</c> with a <see cref="Nameid"/> that already exists.
///
/// In the domain, a <see cref="Nameid"/> must be unique
/// across all users.
/// </summary>
public sealed class UserNameidAlreadyExistsDomainException(Nameid nameid)
    : FargoDomainException($"A user with Nameid '{nameid}' already exists.")
{
    /// <summary>
    /// Gets the <see cref="Nameid"/> that caused the conflict.
    /// </summary>
    public Nameid Nameid { get; } = nameid;
}
/// <summary>
/// Exception thrown when a user attempts to perform an action
/// for which they do not have permission.
/// </summary>
public sealed class UserNotAuthorizedFargoDomainException(
    Guid userGuid,
    ActionType actionType
    ) : FargoDomainException(
        $"User '{userGuid}' is not authorized to perform action '{actionType}'.")
{
    /// <summary>
    /// Gets the identifier of the user that attempted the action.
    /// </summary>
    public Guid UserGuid { get; } = userGuid;

    /// <summary>
    /// Gets the action the user attempted to perform.
    /// </summary>
    public ActionType ActionType { get; } = actionType;
}

public sealed class UserEntityAccessNotAuthorizedFargoDomainException(
    Guid userGuid,
    Guid entityGuid
    ) : FargoDomainException(
        $"User '{userGuid}' is not authorized to access entity '{entityGuid}'.")
{
    /// <summary>
    /// Gets the identifier of the user that attempted the action.
    /// </summary>
    public Guid UserGuid { get; } = userGuid;

    public Guid EntityGuid { get; } = entityGuid;
}

public sealed class UserPartitionAccessNotAuthorizedFargoDomainException(
    Guid userGuid,
    Guid partitionGuid
    ) : FargoDomainException(
        $"User '{userGuid}' is not authorized to access partition '{partitionGuid}'.")
{
    /// <summary>
    /// Gets the identifier of the user that attempted the action.
    /// </summary>
    public Guid UserGuid { get; } = userGuid;

    public Guid PartitionGuid { get; } = partitionGuid;
}
