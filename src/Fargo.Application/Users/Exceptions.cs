using Fargo.Core;

namespace Fargo.Application.Users;

/// <summary>
/// Represents an exception that is thrown when a user cannot be found.
/// </summary>
/// <remarks>
/// This exception may occur when attempting to retrieve a user
/// by its unique identifier or by its <see cref="Nameid"/>.
/// </remarks>
public class UserNotFoundFargoApplicationException
    : FargoApplicationException
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="UserNotFoundFargoApplicationException"/> class
    /// for a missing user identified by a unique identifier.
    /// </summary>
    /// <param name="userGuid">
    /// The unique identifier of the user that was not found.
    /// </param>
    public UserNotFoundFargoApplicationException(Guid userGuid)
        : base($"User with guid '{userGuid}' was not found.")
    {
        UserGuid = userGuid;
    }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="UserNotFoundFargoApplicationException"/> class
    /// for a missing user identified by a <see cref="Nameid"/>.
    /// </summary>
    /// <param name="nameid">
    /// The <see cref="Nameid"/> of the user that was not found.
    /// </param>
    public UserNotFoundFargoApplicationException(Nameid nameid)
        : base($"User with nameid '{nameid}' was not found.")
    {
        Nameid = nameid;
    }

    /// <summary>
    /// Gets the unique identifier of the user that was not found.
    /// </summary>
    /// <remarks>
    /// This value is populated when the exception was created
    /// using a user guid.
    /// </remarks>
    public Guid? UserGuid { get; }

    /// <summary>
    /// Gets the <see cref="Nameid"/> of the user that was not found.
    /// </summary>
    /// <remarks>
    /// This value is populated when the exception was created
    /// using a <see cref="Nameid"/>.
    /// </remarks>
    public Nameid? Nameid { get; }
}

/// <summary>
/// Represents an exception that is thrown when a user attempts
/// to perform an action without the required authorization.
/// </summary>
/// <remarks>
/// This exception indicates that the user exists, but does not
/// have permission to execute the requested <see cref="ActionType"/>.
/// </remarks>
/// <param name="userGuid">
/// The unique identifier of the user that attempted the action.
/// </param>
/// <param name="actionType">
/// The action that the user is not authorized to perform.
/// </param>
public sealed class UserNotAuthorizedFargoApplicationException(
    Guid userGuid,
    ActionType actionType
) : FargoApplicationException(
    $"User '{userGuid}' is not authorized to perform action '{actionType}'.")
{
    /// <summary>
    /// Gets the unique identifier of the unauthorized user.
    /// </summary>
    public Guid UserGuid { get; } = userGuid;

    /// <summary>
    /// Gets the action that the user attempted to perform
    /// without authorization.
    /// </summary>
    public ActionType ActionType { get; } = actionType;
}
