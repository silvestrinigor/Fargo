using Fargo.Application.Common;
using Fargo.Core.Common;

namespace Fargo.Application.Identity;

/// <summary>
/// Exception thrown when a user must change their password before accessing the system.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UserPasswordChangeRequiredFargoApplicationException"/> class.
/// </remarks>
/// <param name="userGuid">
/// The identifier of the user who must change their password.
/// </param>
public sealed class UserPasswordChangeRequiredFargoApplicationException(Guid userGuid)
    : FargoApplicationException($"User '{userGuid}' must change their password before continuing.", FargoErrorType.NotAuthorized)
{
    /// <summary>
    /// Gets the identifier of the user who must change their password.
    /// </summary>
    public Guid UserGuid { get; } = userGuid;
}
