namespace Fargo.Domain.Users;

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
