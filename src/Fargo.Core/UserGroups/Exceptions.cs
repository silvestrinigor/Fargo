namespace Fargo.Core.UserGroups;

/// <summary>
/// Exception thrown when an attempt is made to delete the default administrators user group.
/// </summary>
/// <remarks>
/// The default administrators user group is a critical system entity and cannot be deleted.
/// </remarks>
public sealed class DeleteDefaultAdministratorsUserGroupFargoDomainException()
    : FargoDomainException("The default administrators user group cannot be deleted.")
{
}
/// <summary>
/// Exception thrown when an operation requires an active user group,
/// but the specified group is inactive.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UserGroupInactiveFargoDomainException"/> class.
/// </remarks>
/// <param name="userGroupGuid">
/// The unique identifier of the inactive user group.
/// </param>
public sealed class UserGroupInactiveFargoDomainException(Guid userGroupGuid)
    : FargoDomainException($"The user group '{userGroupGuid}' is inactive.")
{
    /// <summary>
    /// Gets the unique identifier of the inactive user group.
    /// </summary>
    public Guid UserGroupGuid { get; } = userGroupGuid;
}
/// <summary>
/// Exception thrown when attempting to create a <see cref="Fargo.Core.UserGroups.UserGroup"/>
/// with a <see cref="Nameid"/> that already exists in the system.
/// </summary>
public sealed class UserGroupNameidAlreadyExistsDomainException(
    Nameid nameid
    ) : FargoDomainException(
        $"A user group with nameid '{nameid}' already exists.")
{
    /// <summary>
    /// Gets the conflicting <see cref="Nameid"/>.
    /// </summary>
    public Nameid Nameid { get; } = nameid;
}
