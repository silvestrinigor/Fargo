using Fargo.Application.Exceptions;
using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Services;

namespace Fargo.Application.Helpers;

/// <summary>
/// Provides helper methods for validating user permissions.
/// </summary>
/// <remarks>
/// This class centralizes permission validation logic to ensure consistent
/// enforcement of authorization rules across the application.
///
/// Permission evaluation is delegated to <see cref="UserService"/>.
/// </remarks>
public static class UserPermissionHelper
{
    /// <summary>
    /// Validates that the specified <paramref name="user"/> has permission
    /// to perform the given <paramref name="action"/>.
    /// </summary>
    /// <param name="user">
    /// The user attempting to perform the action.
    /// </param>
    /// <param name="action">
    /// The action being performed.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="user"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="UserNotAuthorizedFargoApplicationException">
    /// Thrown when the user does not have permission to perform the action.
    /// </exception>
    /// <remarks>
    /// This method should be used by application layer operations before
    /// executing actions that require specific permissions.
    /// </remarks>
    public static void ValidateHasPermission(User user, ActionType action)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (!UserService.HasPermission(user, action))
        {
            throw new UserNotAuthorizedFargoApplicationException(
                user.Guid,
                action);
        }
    }
}
