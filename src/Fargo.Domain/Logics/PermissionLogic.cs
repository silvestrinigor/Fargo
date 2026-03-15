using Fargo.Domain.Enums;

namespace Fargo.Domain.Logics;

/// <summary>
/// Represents an object that grants a specific permission action.
/// </summary>
/// <remarks>
/// This abstraction allows different permission sources, such as direct user
/// permissions or group permissions, to be evaluated uniformly.
/// </remarks>
public interface IPermission
{
    /// <summary>
    /// Gets the action granted by the permission.
    /// </summary>
    ActionType Action { get; }
}

/// <summary>
/// Represents an object that exposes a read-only collection of permissions.
/// </summary>
/// <remarks>
/// Implementations typically include domain entities such as users or user groups
/// that participate in authorization checks.
/// </remarks>
public interface IUserWithPermissions
{
    /// <summary>
    /// Gets the read-only collection of permissions associated with the object.
    /// </summary>
    IReadOnlyCollection<IPermission> Permissions { get; }
}

/// <summary>
/// Provides extension methods for evaluating permissions.
/// </summary>
public static class PermissionLogic
{
    /// <summary>
    /// Determines whether the specified object has the given permission.
    /// </summary>
    /// <param name="user">The object whose permissions will be evaluated.</param>
    /// <param name="action">The action to verify.</param>
    /// <returns>
    /// <see langword="true"/> if the specified object has the given permission;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="user"/> is <see langword="null"/>.
    /// </exception>
    public static bool HasPermission(this IUserWithPermissions user, ActionType action)
    {
        ArgumentNullException.ThrowIfNull(user);

        return user.Permissions.Any(p => p.Action == action);
    }
}