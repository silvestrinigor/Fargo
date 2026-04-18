namespace Fargo.Application.Exceptions;

/// <summary>
/// Exception thrown when a user group with the specified identifier
/// cannot be found in the system.
/// </summary>
/// <remarks>
/// Initializes a new instance of the
/// <see cref="UserGroupNotFoundFargoApplicationException"/> class.
/// </remarks>
/// <param name="userGroupGuid">
/// The unique identifier of the user group that was requested.
/// </param>
public sealed class UserGroupNotFoundFargoApplicationException(Guid userGroupGuid)
            : FargoApplicationException($"User group '{userGroupGuid}' was not found.")
{
    /// <summary>
    /// Gets the unique identifier of the user group that could not be found.
    /// </summary>
    public Guid UserGroupGuid
    {
        get;
    } = userGroupGuid;
}
