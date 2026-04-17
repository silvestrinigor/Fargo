namespace Fargo.Domain.Users;

// TODO: validate documentation
/// <summary>
/// Represents an object that exposes a read-only collection of permissions.
/// </summary>
/// <remarks>
/// Implementations typically include domain entities such as users or user groups
/// that participate in authorization checks.
/// </remarks>
public interface IPermissionUser
{
    /// <summary>
    /// Gets the read-only collection of permissions associated with the object.
    /// </summary>
    IReadOnlyCollection<IPermission> Permissions { get; }
}
