namespace Fargo.Domain.Users;

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
