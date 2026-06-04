using Fargo.Core.Shared;

namespace Fargo.Core.UserGroups;

/// <summary>
/// Exception thrown when an attempt is made to delete the default administrators user group.
/// </summary>
/// <remarks>
/// The default administrators user group is a critical system entity and cannot be deleted.
/// </remarks>
public sealed class DeleteDefaultAdministratorsUserGroupFargoDomainException()
    : Exception("The default administrators user group cannot be deleted.")
{
}

/// <summary>
/// Exception thrown when attempting to create a <see cref="Fargo.Core.UserGroups.UserGroup"/>
/// with a <see cref="Nameid"/> that already exists in the system.
/// </summary>
public sealed class UserGroupNameidAlreadyExistsDomainException(
    Nameid nameid
    ) : Exception(
        $"A user group with nameid '{nameid}' already exists.")
{
    /// <summary>
    /// Gets the conflicting <see cref="Nameid"/>.
    /// </summary>
    public Nameid Nameid { get; } = nameid;
}
