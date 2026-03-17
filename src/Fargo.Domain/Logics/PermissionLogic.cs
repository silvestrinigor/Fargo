using Fargo.Domain.Entities;
using Fargo.Domain.Enums;

namespace Fargo.Domain.Logics;

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
    public static bool HasPermission(this IPermissionUser user, ActionType action)
    {
        ArgumentNullException.ThrowIfNull(user);

        return user.Permissions.Any(p => p.Action == action);
    }
}
