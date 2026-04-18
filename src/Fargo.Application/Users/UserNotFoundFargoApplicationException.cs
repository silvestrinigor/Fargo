using Fargo.Domain.Users;

namespace Fargo.Application.Users;

/// <summary>
/// Exception thrown when a user cannot be found.
/// </summary>
public class UserNotFoundFargoApplicationException
    : FargoApplicationException
{
    /// <summary>
    /// Initializes a new instance when a user cannot be found by Guid.
    /// </summary>
    public UserNotFoundFargoApplicationException(Guid userGuid)
        : base($"User with guid '{userGuid}' was not found.")
    {
        UserGuid = userGuid;
    }

    /// <summary>
    /// Initializes a new instance when a user cannot be found by Nameid.
    /// </summary>
    public UserNotFoundFargoApplicationException(Nameid nameid)
        : base($"User with nameid '{nameid}' was not found.")
    {
        Nameid = nameid;
    }

    /// <summary>
    /// Gets the Guid used in the lookup.
    /// </summary>
    public Guid? UserGuid { get; }

    /// <summary>
    /// Gets the Nameid used in the lookup.
    /// </summary>
    public Nameid? Nameid { get; }
}
